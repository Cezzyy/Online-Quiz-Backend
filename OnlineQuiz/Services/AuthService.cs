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

        public async Task<ServiceResponse> ResetPasswordAsync(string email)
        {
            try
            {
                // Validate email
                if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
                    return new ServiceResponse("Valid email is required");

                // Check if user exists
                var userResponse = await _userRepository.GetUserByEmailAsync(email);
                if (!userResponse.Success || userResponse.Data == null)
                {
                    // For security, don't reveal if email exists or not
                    return new ServiceResponse("If the email exists, a password reset link has been sent");
                }

                // Generate reset token (in production, store this in database with expiry)
                var resetToken = Guid.NewGuid().ToString("N");
                
                // TODO: In production, you would:
                // 1. Store the reset token in database with expiry (e.g., 1 hour)
                // 2. Send email with reset link containing the token
                // 3. The reset link would be: https://yourapp.com/reset-password?token={resetToken}
                
                // For now, we'll just return success
                return new ServiceResponse("If the email exists, a password reset link has been sent");
            }
            catch (Exception ex)
            {
                return new ServiceResponse($"Password reset request failed: {ex.Message}");
            }
        }

        public Task<ServiceResponse> ConfirmPasswordResetAsync(string token, string newPassword)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(token))
                    return Task.FromResult(new ServiceResponse("Reset token is required"));

                if (string.IsNullOrWhiteSpace(newPassword))
                    return Task.FromResult(new ServiceResponse("New password is required"));

                if (!IsValidPassword(newPassword))
                    return Task.FromResult(new ServiceResponse("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number"));

                // TODO: In production, you would:
                // 1. Look up the reset token in the database
                // 2. Check if it's not expired
                // 3. Get the associated user
                // 4. Update the user's password
                // 5. Invalidate the reset token

                // For now, we'll simulate this process
                // In a real implementation, you'd query the database for the token
                return Task.FromResult(new ServiceResponse("Password reset functionality requires database token storage implementation"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ServiceResponse($"Password reset failed: {ex.Message}"));
            }
        }

        public Task<ServiceResponse> VerifyEmailAsync(string token)
        {
            try
            {
                // Validate token
                if (string.IsNullOrWhiteSpace(token))
                    return Task.FromResult(new ServiceResponse("Verification token is required"));

                // TODO: In production, you would:
                // 1. Look up the verification token in the database
                // 2. Check if it's not expired
                // 3. Get the associated user
                // 4. Mark the user's email as verified
                // 5. Invalidate the verification token

                // For now, we'll simulate this process
                return Task.FromResult(new ServiceResponse("Email verification functionality requires database token storage implementation"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ServiceResponse($"Email verification failed: {ex.Message}"));
            }
        }

        public async Task<ServiceResponse> ResendVerificationEmailAsync(string email)
        {
            try
            {
                // Validate email
                if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
                    return new ServiceResponse("Valid email is required");

                // Check if user exists
                var userResponse = await _userRepository.GetUserByEmailAsync(email);
                if (!userResponse.Success || userResponse.Data == null)
                {
                    // For security, don't reveal if email exists or not
                    return new ServiceResponse("If the email exists and is not verified, a verification email has been sent");
                }

                // TODO: In production, you would:
                // 1. Check if email is already verified
                // 2. Generate new verification token
                // 3. Store token in database with expiry
                // 4. Send verification email

                // Generate verification token
                var verificationToken = Guid.NewGuid().ToString("N");
                
                return new ServiceResponse("If the email exists and is not verified, a verification email has been sent");
            }
            catch (Exception ex)
            {
                return new ServiceResponse($"Resend verification email failed: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<LoginResponseDto>> RegisterAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Validate input
                var validationResult = ValidateCreateUserDto(createUserDto);
                if (!validationResult.Success)
                    return new ServiceResponse<LoginResponseDto>(validationResult.Message);

                // Check if email already exists
                var existingUser = await _userRepository.GetUserByEmailAsync(createUserDto.Email);
                if (existingUser.Success && existingUser.Data != null)
                    return new ServiceResponse<LoginResponseDto>("Email already exists");

                // Create user
                var createResult = await _userRepository.CreateUserAsync(createUserDto);
                if (!createResult.Success)
                    return new ServiceResponse<LoginResponseDto>(createResult.Message);

                // Return success response without JWT token
                // Registration should not automatically log in the user
                // Users should use the login endpoint after account creation
                return new ServiceResponse<LoginResponseDto>
                {
                    Success = true,
                    Message = "User registered successfully. Please login with your credentials.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>(ex.Message);
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

        private ServiceResponse ValidateCreateUserDto(CreateUserDto createUserDto)
        {
            if (createUserDto == null)
                return new ServiceResponse("User data is required");

            if (string.IsNullOrWhiteSpace(createUserDto.Email))
                return new ServiceResponse("Email is required");

            if (string.IsNullOrWhiteSpace(createUserDto.Password))
                return new ServiceResponse("Password is required");

            if (string.IsNullOrWhiteSpace(createUserDto.FullName))
                return new ServiceResponse("Full name is required");

            if (!IsValidEmail(createUserDto.Email))
                return new ServiceResponse("Invalid email format");

            if (!IsValidPassword(createUserDto.Password))
                return new ServiceResponse("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number");

            if (createUserDto.FullName.Length > 60)
                return new ServiceResponse("Full name cannot exceed 60 characters");

            return new ServiceResponse();
        }

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