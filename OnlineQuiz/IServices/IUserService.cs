using OnlineQuiz.Models;
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
        Task<ServiceResponse> AssignRoleAsync(long userId, string roleName);
        Task<ServiceResponse> RemoveRoleAsync(long userId, string roleName);
        Task<ServiceResponse<IEnumerable<RoleModel>>> GetUserRolesAsync(long userId);
        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersByRoleAsync(string roleName);
        Task<ServiceResponse<bool>> IsEmailAvailableAsync(string email);
        Task<ServiceResponse<UserDto>> GetUserProfileAsync(long userId);
        Task<ServiceResponse<LoginResponseDto>> AuthenticateAsync(LoginDto loginDto);
        Task<ServiceResponse<string>> GenerateJwtTokenAsync(UserDto user);
    }
}