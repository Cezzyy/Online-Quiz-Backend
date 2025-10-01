using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;
using System.Threading.Tasks;

namespace OnlineQuiz.IServices
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> AuthenticateAsync(LoginDto loginDto);
        Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    }
}