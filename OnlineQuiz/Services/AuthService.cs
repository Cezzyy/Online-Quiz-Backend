using OnlineQuiz.IRepository;
using OnlineQuiz.IServices;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;
using OnlineQuiz.Data;
using OnlineQuiz.Configuration;
using OnlineQuiz.Utilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineQuiz.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IConfiguration _configuration;
        private readonly OnlineQuizDbContext _context;

        public AuthService(ILoginRepository loginRepository, IConfiguration configuration, OnlineQuizDbContext context)
        {
            _loginRepository = loginRepository;
            _configuration = configuration;
            _context = context;
        }

        public async Task<ServiceResponse<LoginResponseDto>> AuthenticateAsync(LoginDto loginDto)
        {
            try
            {
                // Validate input
                if (loginDto == null)
                    return new ServiceResponse<LoginResponseDto>("Login data is required");

                if (string.IsNullOrWhiteSpace(loginDto.Email))
                    return new ServiceResponse<LoginResponseDto>("Email is required");

                if (string.IsNullOrWhiteSpace(loginDto.Password))
                    return new ServiceResponse<LoginResponseDto>("Password is required");

                if (!IsValidEmail(loginDto.Email))
                    return new ServiceResponse<LoginResponseDto>("Invalid email format");

                // Authenticate user
                var authResult = await _loginRepository.AuthenticateAsync(loginDto);
                if (authResult is null || !authResult.Success || authResult.Data?.User == null)
                {
                    var msg = authResult?.Message ?? "Invalid email or password";
                    return new ServiceResponse<LoginResponseDto>(msg);
                }

                var userDto = authResult.Data.User;
                
                // Fetch UserModel from database for JWT generation
                var userModel = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userDto.Id);
                
                if (userModel == null)
                    return new ServiceResponse<LoginResponseDto>("User not found");
                
                // Generate new access token (15 minutes) and refresh token (7 days)
                var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
                var roles = userModel.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? [];
                
                var accessToken = JwtTokenHelper.GenerateToken(userModel, roles, jwtSettings);
                var refreshToken = JwtTokenHelper.GenerateRefreshToken();
                
                // Hash the refresh token before storing (security best practice)
                var pepper = RefreshTokenHasher.GetPepper();
                var tokenHash = RefreshTokenHasher.HashToken(refreshToken, pepper);
                
                // Store only the hash in database (never store plaintext tokens)
                var refreshTokenEntity = new RefreshTokenModel
                {
                    TokenHash = tokenHash,
                    UserId = userModel.UserId,
                    ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationInDays),
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();
                
                // Create clean user summary (no PII)
                var userSummary = new UserSummaryDto
                {
                    Id = userDto.Id,
                    Email = userDto.Email,
                    FullName = userDto.FullName,
                    Roles = roles
                };
                
                // Create response with tokens and expiresIn (seconds)
                var loginResponse = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken, // Will be removed for web clients in controller
                    TokenType = "Bearer",
                    ExpiresIn = jwtSettings.AccessTokenExpirationInMinutes * 60, // Convert to seconds
                    RefreshExpiresIn = jwtSettings.RefreshTokenExpirationInDays * 24 * 60 * 60, // Convert to seconds
                    User = userSummary
                };
                
                return new ServiceResponse<LoginResponseDto>(loginResponse);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UserModel>> ValidateUserCredentialsAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceResponse<UserModel>("Email is required");

                if (string.IsNullOrWhiteSpace(password))
                    return new ServiceResponse<UserModel>("Password is required");

                if (!IsValidEmail(email))
                    return new ServiceResponse<UserModel>("Invalid email format");

                return await _loginRepository.ValidateUserCredentialsAsync(email, password);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserModel>(ex.Message);
            }
        }

        public async Task<ServiceResponse> LogoutAsync(long userId)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse("Invalid user ID");

                // Revoke all active refresh tokens for this user
                var activeTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                    .ToListAsync();

                if (activeTokens.Any())
                {
                    foreach (var token in activeTokens)
                    {
                        token.RevokedAt = DateTime.UtcNow;
                    }
                    
                    await _context.SaveChangesAsync();
                }

                return new ServiceResponse("Logged out successfully");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public Task<ServiceResponse<string>> GenerateJwtTokenAsync(UserModel user)
        {
            try
            {
                if (user == null)
                    return Task.FromResult(new ServiceResponse<string>("User is required"));

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

                if (string.IsNullOrWhiteSpace(secretKey))
                    return Task.FromResult(new ServiceResponse<string>("JWT secret key not configured"));

                var key = Encoding.ASCII.GetBytes(secretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity([
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Email, user.Email ?? ""),
                        new Claim(ClaimTypes.Name, user.FullName ?? ""),
                        new Claim("userId", user.UserId.ToString()),
                    ]),
                    Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Task.FromResult(new ServiceResponse<string>(tokenString));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ServiceResponse<string>(ex.Message));
            }
        }

        public async Task<ServiceResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
                    return new ServiceResponse<RefreshTokenResponseDto>("Refresh token is required");

                // Get pepper for token verification
                var pepper = RefreshTokenHasher.GetPepper();
                
                // Get all active refresh tokens for verification
                // We need to check all active tokens since we can't query by hash directly
                var activeRefreshTokens = await _context.RefreshTokens
                    .Include(rt => rt.User)
                        .ThenInclude(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                    .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                // Find the matching token using secure hash verification
                RefreshTokenModel? matchingToken = null;
                foreach (var token in activeRefreshTokens)
                {
                    if (RefreshTokenHasher.VerifyToken(refreshTokenDto.RefreshToken, token.TokenHash, pepper))
                    {
                        matchingToken = token;
                        break;
                    }
                }

                if (matchingToken == null)
                    return new ServiceResponse<RefreshTokenResponseDto>("Invalid or expired refresh token");

                var user = matchingToken.User;
                var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
                var roles = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? [];

                // Generate new access token and refresh token
                var newAccessToken = JwtTokenHelper.GenerateToken(user, roles, jwtSettings);
                var newRefreshToken = JwtTokenHelper.GenerateRefreshToken();
                
                // Hash the new refresh token
                var newTokenHash = RefreshTokenHasher.HashToken(newRefreshToken, pepper);

                // Revoke old refresh token
                matchingToken.RevokedAt = DateTime.UtcNow;

                // Create new refresh token with hash
                var newRefreshTokenEntity = new RefreshTokenModel
                {
                    TokenHash = newTokenHash,
                    UserId = (int)user.UserId,
                    ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationInDays),
                    CreatedAt = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(newRefreshTokenEntity);
                await _context.SaveChangesAsync();

                var response = new RefreshTokenResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = jwtSettings.AccessTokenExpirationInMinutes * 60,
                    RefreshExpiresIn = jwtSettings.RefreshTokenExpirationInDays * 24 * 60 * 60
                };

                return new ServiceResponse<RefreshTokenResponseDto>(response);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RefreshTokenResponseDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<string>> GenerateRefreshTokenAsync(UserModel user)
        {
            try
            {
                if (user == null)
                    return new ServiceResponse<string>("User is required");

                var refreshToken = JwtTokenHelper.GenerateRefreshToken();
                var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

                // Hash the refresh token before storing
                var pepper = RefreshTokenHasher.GetPepper();
                var tokenHash = RefreshTokenHasher.HashToken(refreshToken, pepper);

                var refreshTokenEntity = new RefreshTokenModel
                {
                    TokenHash = tokenHash,
                    UserId = (int)user.UserId,
                    ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationInDays),
                    CreatedAt = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                return new ServiceResponse<string>(refreshToken);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(ex.Message);
            }
        }

        public async Task<ServiceResponse> RevokeRefreshTokenAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return new ServiceResponse("Refresh token is required");

                // Get pepper for token verification
                var pepper = RefreshTokenHasher.GetPepper();
                
                // Get all active refresh tokens for verification
                var activeRefreshTokens = await _context.RefreshTokens
                    .Where(rt => !rt.IsRevoked)
                    .ToListAsync();

                // Find the matching token using secure hash verification
                RefreshTokenModel? matchingToken = null;
                foreach (var token in activeRefreshTokens)
                {
                    if (RefreshTokenHasher.VerifyToken(refreshToken, token.TokenHash, pepper))
                    {
                        matchingToken = token;
                        break;
                    }
                }

                if (matchingToken == null)
                    return new ServiceResponse("Refresh token not found");

                if (matchingToken.IsRevoked)
                    return new ServiceResponse("Refresh token already revoked");

                matchingToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ServiceResponse();
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        #region Private Helper Methods

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}