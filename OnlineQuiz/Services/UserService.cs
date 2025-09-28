using OnlineQuiz.IRepository;
using OnlineQuiz.IServices;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;
using OnlineQuiz.Utilities;
using OnlineQuiz.Data;
using Microsoft.EntityFrameworkCore;

namespace OnlineQuiz.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly OnlineQuizDbContext _context;

        public UserService(IUserRepository userRepository, IAuthService authService, OnlineQuizDbContext context)
        {
            _userRepository = userRepository;
            _authService = authService;
            _context = context;
        }

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                return await _userRepository.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<UserDto>>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UserDto>> GetUserByIdAsync(long userId)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse<UserDto>("Invalid user ID");

                return await _userRepository.GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceResponse<UserDto>("Email is required");

                if (!IsValidEmail(email))
                    return new ServiceResponse<UserDto>("Invalid email format");

                return await _userRepository.GetUserByEmailAsync(email);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Validate input
                var validationResult = ValidateCreateUserDto(createUserDto);
                if (!validationResult.Success)
                    return new ServiceResponse<UserDto>(validationResult.Message);

                // Check if email already exists
                var existingUser = await _userRepository.GetUserByEmailAsync(createUserDto.Email);
                if (existingUser.Success && existingUser.Data != null)
                    return new ServiceResponse<UserDto>("Email already exists");

                // Validate password strength
                if (!IsValidPassword(createUserDto.Password))
                    return new ServiceResponse<UserDto>("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number");

                return await _userRepository.CreateUserAsync(createUserDto);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(long userId, UpdateUserDto updateUserDto)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse<UserDto>("Invalid user ID");

                // Check if user exists
                var existingUser = await _userRepository.GetUserByIdAsync(userId);
                if (!existingUser.Success || existingUser.Data == null)
                    return new ServiceResponse<UserDto>("User not found");

                // Validate input
                var validationResult = ValidateUpdateUserDto(updateUserDto);
                if (!validationResult.Success)
                    return new ServiceResponse<UserDto>(validationResult.Message);

                return await _userRepository.UpdateUserAsync(userId, updateUserDto);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse> DeleteUserAsync(long userId)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse("Invalid user ID");

                // Check if user exists
                var existingUser = await _userRepository.GetUserByIdAsync(userId);
                if (!existingUser.Success || existingUser.Data == null)
                    return new ServiceResponse("User not found");

                return await _userRepository.DeleteUserAsync(userId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }



        public async Task<ServiceResponse> AssignRoleAsync(long userId, string roleName)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse("Invalid user ID");

                if (string.IsNullOrWhiteSpace(roleName))
                    return new ServiceResponse("Invalid role name");

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                    return new ServiceResponse("Role not found");

                return await _userRepository.AssignRoleAsync(userId, role.RoleId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse> RemoveRoleAsync(long userId, string roleName)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse("Invalid user ID");

                if (string.IsNullOrWhiteSpace(roleName))
                    return new ServiceResponse("Invalid role name");

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                    return new ServiceResponse("Role not found");

                return await _userRepository.RemoveRoleAsync(userId, role.RoleId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<RoleModel>>> GetUserRolesAsync(long userId)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse<IEnumerable<RoleModel>>("Invalid user ID");

                return await _userRepository.GetUserRolesAsync(userId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<RoleModel>>(ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> IsEmailAvailableAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceResponse<bool>("Email is required");

                if (!IsValidEmail(email))
                    return new ServiceResponse<bool>("Invalid email format");

                var existingUser = await _userRepository.GetUserByEmailAsync(email);
                return new ServiceResponse<bool>(!existingUser.Success || existingUser.Data == null);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UserDto>> GetUserProfileAsync(long userId)
        {
            try
            {
                if (userId <= 0)
                    return new ServiceResponse<UserDto>("Invalid user ID");

                return await _userRepository.GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>(ex.Message);
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

            if (createUserDto.FullName.Length > 60)
                return new ServiceResponse("Full name cannot exceed 60 characters");

            return new ServiceResponse();
        }

        private ServiceResponse ValidateUpdateUserDto(UpdateUserDto updateUserDto)
        {
            if (updateUserDto == null)
                return new ServiceResponse("User data is required");

            if (!string.IsNullOrWhiteSpace(updateUserDto.FullName) && updateUserDto.FullName.Length > 60)
                return new ServiceResponse("Full name cannot exceed 60 characters");

            if (!string.IsNullOrWhiteSpace(updateUserDto.ContactNumber) && updateUserDto.ContactNumber.Length > 30)
                return new ServiceResponse("Contact number cannot exceed 30 characters");

            if (!string.IsNullOrWhiteSpace(updateUserDto.EmergencyContactNumber) && updateUserDto.EmergencyContactNumber.Length > 30)
                return new ServiceResponse("Emergency contact number cannot exceed 30 characters");

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

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    return new ServiceResponse<IEnumerable<UserDto>>("Invalid role name");

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                    return new ServiceResponse<IEnumerable<UserDto>>("Role not found");

                return await _userRepository.GetUsersByRoleAsync(role.RoleId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<UserDto>>(ex.Message);
            }
        }

        public async Task<ServiceResponse<LoginResponseDto>> AuthenticateAsync(LoginDto loginDto)
        {
            try
            {
                // Delegate to AuthService for authentication
                return await _authService.AuthenticateAsync(loginDto);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<string>> GenerateJwtTokenAsync(UserDto user)
        {
            try
            {
                // Convert UserDto to UserModel for token generation
                var userModel = new UserModel
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FullName = user.FullName,
                    Status = user.Status
                };

                // Delegate to AuthService for token generation
                return await _authService.GenerateJwtTokenAsync(userModel);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(ex.Message);
            }
        }
    }
}