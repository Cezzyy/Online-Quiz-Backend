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

        public async Task<ServiceResponse> LogoutAsync(long userId)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse("Invalid user ID");

                // For JWT tokens, logout is typically handled client-side by removing the token
                // However, you could implement token blacklisting here if needed
                return new ServiceResponse("Logged out successfully");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return new ServiceResponse("Refresh token is required");

                // Implement refresh token logic here
                // This would typically involve validating the refresh token and generating a new access token
                return new ServiceResponse("Refresh token functionality not implemented");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse> ResetPasswordAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceResponse("Email is required");

                if (!IsValidEmail(email))
                    return new ServiceResponse("Invalid email format");

                // Check if user exists
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (!user.Success || user.Data == null)
                    return new ServiceResponse("If the email exists in our system, a password reset link has been sent");

                // Generate password reset token and send email
                // This would typically involve generating a secure token and sending an email
                return new ServiceResponse("If the email exists in our system, a password reset link has been sent");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse> ConfirmPasswordResetAsync(string token, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return new ServiceResponse("Reset token is required");

                if (string.IsNullOrWhiteSpace(newPassword))
                    return new ServiceResponse("New password is required");

                if (!IsValidPassword(newPassword))
                    return new ServiceResponse("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number");

                // Validate token and reset password
                // This would typically involve validating the reset token and updating the password
                return new ServiceResponse("Password reset functionality not implemented");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse> VerifyEmailAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return new ServiceResponse("Verification token is required");

                // Validate email verification token
                // This would typically involve validating the token and marking the email as verified
                return new ServiceResponse("Email verification functionality not implemented");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse> ResendVerificationEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceResponse("Email is required");

                if (!IsValidEmail(email))
                    return new ServiceResponse("Invalid email format");

                // Check if user exists
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (!user.Success || user.Data == null)
                    return new ServiceResponse("User not found");

                // Resend verification email
                // This would typically involve generating a new verification token and sending an email
                return new ServiceResponse("Verification email resent successfully");
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
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

                // Generate JWT token for the new user
                var user = await _userRepository.GetUserByEmailAsync(createUserDto.Email);
                if (!user.Success || user.Data == null)
                    return new ServiceResponse<LoginResponseDto>("Failed to retrieve created user");

                var tokenResult = await GenerateJwtTokenAsync(new UserModel 
                { 
                    UserId = user.Data.UserId, 
                    Email = user.Data.Email, 
                    FullName = user.Data.FullName 
                });

                if (!tokenResult.Success)
                    return new ServiceResponse<LoginResponseDto>(tokenResult.Message);

                var response = new LoginResponseDto
                {
                    Token = tokenResult.Data,
                    User = user.Data
                };

                return new ServiceResponse<LoginResponseDto>(response);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<string>> GenerateJwtTokenAsync(UserModel user)
        {
            try
            {
                if (user == null)
                    return new ServiceResponse<string>("User is required");

                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

                if (string.IsNullOrWhiteSpace(secretKey))
                    return new ServiceResponse<string>("JWT secret key not configured");

                var key = Encoding.ASCII.GetBytes(secretKey);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
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

                return new ServiceResponse<string>(tokenString);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(ex.Message);
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