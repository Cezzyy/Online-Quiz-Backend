using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using OnlineQuiz.Data;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Utilities;
using OnlineQuiz.Configuration;

namespace OnlineQuiz.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly OnlineQuizDbContext _context;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public UserRepository(OnlineQuizDbContext context, IMapper mapper, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .ToListAsync();

                var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
                
                return new ServiceResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Data = userDtos,
                    Message = "Users retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<UserDto>>
                {
                    Success = false,
                    Message = $"Error retrieving users: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserDto>> GetUserByIdAsync(long userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return new ServiceResponse<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var userDto = _mapper.Map<UserDto>(user);
                
                return new ServiceResponse<UserDto>
                {
                    Success = true,
                    Data = userDto,
                    Message = "User retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>
                {
                    Success = false,
                    Message = $"Error retrieving user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return new ServiceResponse<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var userDto = _mapper.Map<UserDto>(user);
                
                return new ServiceResponse<UserDto>
                {
                    Success = true,
                    Data = userDto,
                    Message = "User retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>
                {
                    Success = false,
                    Message = $"Error retrieving user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == createUserDto.Email);

                if (existingUser != null)
                {
                    return new ServiceResponse<UserDto>
                    {
                        Success = false,
                        Message = "User with this email already exists"
                    };
                }

                // Hash password
                var hashedPassword = PasswordHelper.HashPassword(createUserDto.Password);

                // Create new user
                var user = new UserModel
                {
                    Email = createUserDto.Email,
                    PasswordHash = hashedPassword,
                    FullName = createUserDto.FullName,
                    ContactNumber = createUserDto.ContactNumber,
                    EmergencyContactNumber = createUserDto.EmergencyContactNumber,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Assign roles if provided
                if (createUserDto.Roles.Any())
                {
                    foreach (var roleName in createUserDto.Roles)
                    {
                        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                        if (role != null)
                        {
                            var userRole = new UserRoleModel
                            {
                                UserId = user.UserId,
                                RoleId = role.RoleId
                            };
                            _context.UserRoles.Add(userRole);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // Reload user with roles
                var createdUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId);

                var userDto = _mapper.Map<UserDto>(createdUser);
                
                return new ServiceResponse<UserDto>
                {
                    Success = true,
                    Data = userDto,
                    Message = "User created successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>
                {
                    Success = false,
                    Message = $"Error creating user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(long userId, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(updateUserDto.FullName))
                    user.FullName = updateUserDto.FullName;
                
                if (updateUserDto.ContactNumber != null)
                    user.ContactNumber = updateUserDto.ContactNumber;
                
                if (updateUserDto.EmergencyContactNumber != null)
                    user.EmergencyContactNumber = updateUserDto.EmergencyContactNumber;
                
                if (!string.IsNullOrEmpty(updateUserDto.Status))
                    user.Status = updateUserDto.Status;

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload user with roles
                var updatedUser = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                var userDto = _mapper.Map<UserDto>(updatedUser);
                
                return new ServiceResponse<UserDto>
                {
                    Success = true,
                    Data = userDto,
                    Message = "User updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>
                {
                    Success = false,
                    Message = $"Error updating user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse> DeleteUserAsync(long userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return new ServiceResponse
                {
                    Success = true,
                    Message = "User deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = $"Error deleting user: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                if (user.Status != "Active")
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "User account is not active"
                    };
                }

                // Generate JWT token
                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var token = JwtTokenHelper.GenerateToken(user, roles, _jwtSettings);
                var userDto = _mapper.Map<UserDto>(user);

                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    User = userDto
                };

                return new ServiceResponse<LoginResponseDto>
                {
                    Success = true,
                    Data = loginResponse,
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = $"Error during login: {ex.Message}"
                };
            }
        }



        public async Task<ServiceResponse> AssignRoleAsync(long userId, short roleId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "Role not found"
                    };
                }

                var existingUserRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

                if (existingUserRole != null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "User already has this role"
                    };
                }

                var userRole = new UserRoleModel
                {
                    UserId = userId,
                    RoleId = roleId
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                return new ServiceResponse
                {
                    Success = true,
                    Message = "Role assigned successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = $"Error assigning role: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse> RemoveRoleAsync(long userId, short roleId)
        {
            try
            {
                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

                if (userRole == null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "User role not found"
                    };
                }

                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();

                return new ServiceResponse
                {
                    Success = true,
                    Message = "Role removed successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = $"Error removing role: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<RoleModel>>> GetUserRolesAsync(long userId)
        {
            try
            {
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Include(ur => ur.Role)
                    .Select(ur => ur.Role)
                    .ToListAsync();

                return new ServiceResponse<IEnumerable<RoleModel>>
                {
                    Success = true,
                    Data = roles,
                    Message = "User roles retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<RoleModel>>
                {
                    Success = false,
                    Message = $"Error retrieving user roles: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersByRoleAsync(short roleId)
        {
            try
            {
                var users = await _context.UserRoles
                    .Where(ur => ur.RoleId == roleId)
                    .Include(ur => ur.User)
                    .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Select(ur => ur.User)
                    .ToListAsync();

                var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

                return new ServiceResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Data = userDtos,
                    Message = "Users retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<UserDto>>
                {
                    Success = false,
                    Message = $"Error retrieving users by role: {ex.Message}"
                };
            }
        }
    }
}