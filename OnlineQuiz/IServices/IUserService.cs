using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;

namespace OnlineQuiz.IServices
{
    public interface IUserService
    {
        Task<ServiceResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<ServiceResponse<UserDto>> GetUserByIdAsync(long userId);
        Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email);
        Task<ServiceResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<ServiceResponse<UserDto>> UpdateUserAsync(long userId, UpdateUserDto updateUserDto);
        Task<ServiceResponse> DeleteUserAsync(long userId);
        Task<ServiceResponse<object>> GetUsersByRoleAsync(string roleName);
    }
}