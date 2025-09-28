using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;

namespace OnlineQuiz.IRepository
{
    public interface IUserRepository
    {
        Task<ServiceResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<ServiceResponse<UserDto>> GetUserByIdAsync(long userId);
        Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email);
        Task<ServiceResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<ServiceResponse<UserDto>> UpdateUserAsync(long userId, UpdateUserDto updateUserDto);
        Task<ServiceResponse> DeleteUserAsync(long userId);
        Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);

        Task<ServiceResponse> AssignRoleAsync(long userId, short roleId);
        Task<ServiceResponse> RemoveRoleAsync(long userId, short roleId);
        Task<ServiceResponse<IEnumerable<RoleModel>>> GetUserRolesAsync(long userId);
        Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersByRoleAsync(short roleId);
    }
}