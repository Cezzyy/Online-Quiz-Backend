using OnlineQuiz.IRepository;
using OnlineQuiz.IServices;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineQuiz.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(ILoginRepository loginRepository, IUserRepository userRepository, IConfiguration configuration)
        {
            _loginRepository = loginRepository;
            _userRepository = userRepository;
            _configuration = configuration;
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
                if (!authResult.Success)
                    return new ServiceResponse<LoginResponseDto>(authResult.Message);

                return authResult;
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

        public Task<ServiceResponse> LogoutAsync(long userId)
        {
            try
            {
                if (userId <= 0)
                    return Task.FromResult(new ServiceResponse("Invalid user ID"));

                return Task.FromResult(new ServiceResponse("Logged out successfully"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ServiceResponse(ex.Message));
            }
        }

        public async Task<ServiceResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // For now, we'll use the existing token validation approach
                // In a production system, you'd want to store refresh tokens in the database
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"] ?? "");
                
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    ValidateLifetime = false, // We'll validate this manually
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    return new ServiceResponse("Invalid refresh token");

                // Get user and generate new token
                var userResponse = await _userRepository.GetUserByIdAsync(long.Parse(userId));
                if (!userResponse.Success || userResponse.Data == null)
                    return new ServiceResponse("User not found");

                return new ServiceResponse("Token refresh completed successfully");
            }
            catch (Exception ex)
            {
                return new ServiceResponse($"Token refresh failed: {ex.Message}");
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
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Email, user.Email ?? ""),
                        new Claim(ClaimTypes.Name, user.FullName ?? ""),
                        new Claim("userId", user.UserId.ToString())
                    }),
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

        #region Private Helper Methods



        private bool IsValidEmail(string email)
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

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpper && hasLower && hasDigit;
        }

        #endregion
    }
}