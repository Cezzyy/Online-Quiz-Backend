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







        #region Private Helper Methods

        private static ServiceResponse ValidateCreateUserDto(CreateUserDto createUserDto)
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

            // Validate roles
            if (createUserDto.Roles == null || createUserDto.Roles.Count == 0)
                return new ServiceResponse("At least one role must be assigned");

            var validRoles = new[] { "Admin", "Teacher", "Student" };
            var invalidRoles = createUserDto.Roles.Where(role => !validRoles.Contains(role)).ToList();
            if (invalidRoles.Count > 0)
                return new ServiceResponse($"Invalid roles: {string.Join(", ", invalidRoles)}. Valid roles are: Admin, Teacher, Student");

            // Validate Teacher-specific fields
            if (createUserDto.Roles.Contains("Teacher"))
            {
                if (!string.IsNullOrWhiteSpace(createUserDto.Department) && createUserDto.Department.Length > 120)
                    return new ServiceResponse("Department cannot exceed 120 characters");
            }

            // Validate Student-specific fields
            if (createUserDto.Roles.Contains("Student"))
            {
                if (!string.IsNullOrWhiteSpace(createUserDto.StudentNumber) && createUserDto.StudentNumber.Length > 25)
                    return new ServiceResponse("Student number cannot exceed 25 characters");

                if (createUserDto.YearLevel.HasValue && (createUserDto.YearLevel < 1 || createUserDto.YearLevel > 6))
                    return new ServiceResponse("Year level must be between 1 and 6");

                if (!string.IsNullOrWhiteSpace(createUserDto.Section) && createUserDto.Section.Length > 60)
                    return new ServiceResponse("Section cannot exceed 60 characters");

                if (!string.IsNullOrWhiteSpace(createUserDto.Course) && createUserDto.Course.Length > 120)
                    return new ServiceResponse("Course cannot exceed 120 characters");
            }

            return new ServiceResponse();
        }

        private static ServiceResponse ValidateUpdateUserDto(UpdateUserDto updateUserDto)
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

        private static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);

            return hasUpper && hasLower && hasDigit;
        }

        #endregion

        public async Task<ServiceResponse<object>> GetUsersByRoleAsync(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    return new ServiceResponse<object>("Role name is required");

                // Validate that only Student, Teacher, or Admin roles are allowed
                if (roleName != "Student" && roleName != "Teacher" && roleName != "Admin")
                    return new ServiceResponse<object>("Only 'Student', 'Teacher', or 'Admin' roles are allowed");

                // Return appropriate DTO based on role
                if (roleName.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    var teacherResult = await _userRepository.GetAllTeachersWithProfileAsync();
                    if (teacherResult is null || !teacherResult.Success || teacherResult.Data is null)
                    {
                        return ServiceResponse<object>.Fail("No teachers found or lookup failed.");
                    }
                    return new ServiceResponse<object>(teacherResult.Data);
                }
                else if (roleName.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    var studentResult = await _userRepository.GetAllStudentsWithProfileAsync();
                    if (studentResult is null || !studentResult.Success || studentResult.Data is null)
                    {
                        return ServiceResponse<object>.Fail("No students found or lookup failed.");
                    }
                    return new ServiceResponse<object>(studentResult.Data);
                }
                else // Admin role
                {
                    var userResult = await _userRepository.GetUsersByRoleAsync(roleName);
                    if (userResult is null || !userResult.Success || userResult.Data is null)
                    {
                        return ServiceResponse<object>.Fail("No admin users found or lookup failed.");
                    }
                    return new ServiceResponse<object>(userResult.Data);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<object>(ex.Message);
            }
        }


    }
}