using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using System.Security.Claims;

namespace OnlineQuiz.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Tags("Users")]
    [Authorize] // Require authentication for all endpoints
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IActivityLogService _activityLogService;

        public UserController(IUserService userService, IActivityLogService activityLogService)
        {
            _userService = userService;
            _activityLogService = activityLogService;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await _userService.GetAllUsersAsync();
                
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new 
                { 
                    success = true,
                    message = "Users retrieved successfully",
                    data = result.Data,
                    count = result.Data?.ToList().Count ?? 0,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                var result = await _userService.GetUserByIdAsync(id);
                
                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                return Ok(new 
                { 
                    success = true,
                    message = "User retrieved successfully",
                    data = result.Data,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get users by role with complete profile information
        /// </summary>
        /// <param name="roleName">Role name (Student, Teacher, or Admin)</param>
        /// <returns>List of users with the specified role and their complete profile information</returns>
        [HttpGet("role/{roleName}")]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    return BadRequest(new { message = "Role name is required" });
                }

                // Validate that role is Student, Teacher, or Admin
                if (!roleName.Equals("Student", StringComparison.OrdinalIgnoreCase) && 
                    !roleName.Equals("Teacher", StringComparison.OrdinalIgnoreCase) &&
                    !roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Role must be 'Student', 'Teacher', or 'Admin'" });
                }

                var result = await _userService.GetUsersByRoleAsync(roleName);
                
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                // Determine the data type and count based on role
                int count = 0;
                string dataType = "";
                string message = "";
                
                if (roleName.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    var teachers = result.Data as IEnumerable<TeacherDto>;
                    count = teachers?.ToList().Count ?? 0;
                    dataType = "teachers";
                    message = count > 0 
                        ? $"Teachers retrieved successfully with complete profile information. Found {count} teacher(s)."
                        : "No teacher accounts found in the system.";
                }
                else if (roleName.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    var students = result.Data as IEnumerable<StudentDto>;
                    count = students?.ToList().Count ?? 0;
                    dataType = "students";
                    message = count > 0 
                        ? $"Students retrieved successfully with complete profile information. Found {count} student(s)."
                        : "No student accounts found in the system.";
                }
                else // Admin
                {
                    var users = result.Data as IEnumerable<UserDto>;
                    count = users?.ToList().Count ?? 0;
                    dataType = "users";
                    message = count > 0 
                        ? $"Admin users retrieved successfully. Found {count} admin user(s)."
                        : "No admin accounts found in the system.";
                }

                return Ok(new 
                { 
                    success = true,
                    message,
                    roleName,
                    dataType,
                    data = result.Data,
                    count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user details</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (createUserDto == null)
                {
                    return BadRequest(new { message = "User data is required" });
                }

                var result = await _userService.CreateUserAsync(createUserDto);
                
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                // Log activity for user creation
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(currentUserIdClaim, out long currentUserId) && result.Data?.UserId != null)
                {
                    await _activityLogService.LogEntityActionAsync(currentUserId, "CREATE", "User", result.Data.UserId, 
                        $"Created new user: {createUserDto.FullName} ({createUserDto.Email})", null, new { 
                            FullName = createUserDto.FullName, 
                            Email = createUserDto.Email,
                            EmergencyContactPersonName = createUserDto.EmergencyContactPersonName,
                            Bio = createUserDto.Bio,
                            Roles = createUserDto.Roles 
                        });
                }

                return CreatedAtAction(
                    nameof(GetUserById), 
                    new { id = result.Data?.UserId }, 
                    new 
                    { 
                        success = true,
                        message = "User created successfully",
                        data = result.Data,
                        timestamp = DateTime.UtcNow
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">User update data</param>
        /// <returns>Updated user details</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                if (updateUserDto == null)
                {
                    return BadRequest(new { message = "User data is required" });
                }

                var result = await _userService.UpdateUserAsync(id, updateUserDto);
                
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                // Log activity for user update
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (long.TryParse(currentUserIdClaim, out long currentUserId))
                {
                    await _activityLogService.LogEntityActionAsync(currentUserId, "UPDATE", "User", id, 
                        $"Updated user with ID: {id}", null, new { 
                            FullName = updateUserDto.FullName, 
                            ContactNumber = updateUserDto.ContactNumber, 
                            EmergencyContactNumber = updateUserDto.EmergencyContactNumber,
                            EmergencyContactPersonName = updateUserDto.EmergencyContactPersonName,
                            Bio = updateUserDto.Bio,
                            Status = updateUserDto.Status
                        });
                }

                return Ok(new 
                { 
                    success = true,
                    message = "User updated successfully",
                    data = result.Data,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Delete a user (soft delete)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Deletion confirmation</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid user ID" });
                }

                // Get current user ID from JWT token
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                long? currentUserIdLong = null;
                
                if (currentUserId != null && long.TryParse(currentUserId, out var parsedUserId))
                {
                    currentUserIdLong = parsedUserId;
                    
                    // Prevent users from deleting themselves
                    if (currentUserIdLong == id)
                    {
                        return BadRequest(new { message = "You cannot delete your own account" });
                    }
                }

                // Get the user to be deleted and set DeletedBy
                var userToDelete = await _userService.GetUserByIdAsync(id);
                if (!userToDelete.Success || userToDelete.Data == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var result = await _userService.DeleteUserAsync(id, currentUserIdLong);
                
                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                // Log activity for user deletion
                if (currentUserIdLong.HasValue)
                {
                    await _activityLogService.LogEntityActionAsync(currentUserIdLong.Value, "DELETE", "User", id, 
                        $"Soft deleted user with ID: {id}", null, null);
                }

                return Ok(new 
                { 
                    success = true,
                    message = "User deleted successfully (soft delete)",
                    deletedUserId = id,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false,
                    message = "Internal server error", 
                    error = ex.Message 
                });
            }
        }
    }
}