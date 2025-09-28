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
        private readonly IConfiguration _configuration;

        public AuthService(ILoginRepository loginRepository, IConfiguration configuration)
        {
            _loginRepository = loginRepository;
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
                if (authResult is null || !authResult.Success)
                {
                    var msg = authResult?.Message ?? "Invalid email or password";
                    return new ServiceResponse<LoginResponseDto>(msg);
                }

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