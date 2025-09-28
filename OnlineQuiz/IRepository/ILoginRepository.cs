using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;

namespace OnlineQuiz.IRepository
{
    public interface ILoginRepository
    {
        Task<ServiceResponse<LoginResponseDto>> AuthenticateAsync(LoginDto loginDto);
        Task<ServiceResponse<UserModel>> ValidateUserCredentialsAsync(string email, string password);
        Task<ServiceResponse> LogoutAsync(long userId);
        Task<ServiceResponse> RefreshTokenAsync(string refreshToken);

    }
}