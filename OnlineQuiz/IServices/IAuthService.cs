using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;

namespace OnlineQuiz.IServices
{
    public interface IAuthService
    {
        Task<ServiceResponse<LoginResponseDto>> AuthenticateAsync(LoginDto loginDto);
        Task<ServiceResponse<UserModel>> ValidateUserCredentialsAsync(string email, string password);
        Task<ServiceResponse> LogoutAsync(long userId);
        Task<ServiceResponse> RefreshTokenAsync(string refreshToken);
        Task<ServiceResponse> ResetPasswordAsync(string email);
        Task<ServiceResponse> ConfirmPasswordResetAsync(string token, string newPassword);
        Task<ServiceResponse> VerifyEmailAsync(string token);
        Task<ServiceResponse> ResendVerificationEmailAsync(string email);
        Task<ServiceResponse<LoginResponseDto>> RegisterAsync(CreateUserDto createUserDto);
        Task<ServiceResponse<string>> GenerateJwtTokenAsync(UserModel user);
    }
}