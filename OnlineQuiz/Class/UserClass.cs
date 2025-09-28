using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineQuiz.Configuration;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.Utilities;

namespace OnlineQuiz.Class
{
    public class UserClass : IUserRepository
    {
        private readonly OnlineQuizDbContext _context;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public UserClass(OnlineQuizDbContext context, IMapper mapper, IOptions<JwtSettings> jwtSettings)
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
                return new ServiceResponse<IEnumerable<UserDto>>(userDtos);
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
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return new ServiceResponse<UserDto>("User not found");

                var userDto = _mapper.Map<UserDto>(user);
                return new ServiceResponse<UserDto>(userDto);
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
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                    return new ServiceResponse<UserDto>("User not found");

                var userDto = _mapper.Map<UserDto>(user);
                return new ServiceResponse<UserDto>(userDto);
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
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == createUserDto.Email);

                if (existingUser != null)
                    return new ServiceResponse<UserDto>("User with this email already exists");

                var user = _mapper.Map<UserModel>(createUserDto);
                user.PasswordHash = PasswordHelper.HashPassword(createUserDto.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                return new ServiceResponse<UserDto>(userDto);
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
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return new ServiceResponse<UserDto>("User not found");

                _mapper.Map(updateUserDto, user);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                return new ServiceResponse<UserDto>(userDto);
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
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return new ServiceResponse("User not found");

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return new ServiceResponse();
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
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
                    return new ServiceResponse<LoginResponseDto>("Invalid email or password");

                if (user.Status != "Active")
                    return new ServiceResponse<LoginResponseDto>("Account is not active");

                // Get user roles
                var roles = user.UserRoles?.Select(ur => ur.Role.Name) ?? new List<string>();
                
                var token = JwtTokenHelper.GenerateToken(user, roles, _jwtSettings);
                var loginResponse = new LoginResponseDto
                {
                    Token = token,
                    User = _mapper.Map<UserDto>(user)
                };

                return new ServiceResponse<LoginResponseDto>(loginResponse);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>(ex.Message);
            }
        }



        public async Task<ServiceResponse> AssignRoleAsync(long userId, short roleId)
        {
            try
            {
                var userRole = new UserRoleModel
                {
                    UserId = userId,
                    RoleId = (short)roleId
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();

                return new ServiceResponse();
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }

        public async Task<ServiceResponse> RemoveRoleAsync(long userId, short roleId)
        {
            try
            {
                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

                if (userRole == null)
                    return new ServiceResponse("User role not found");

                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();

                return new ServiceResponse();
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
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Include(ur => ur.Role)
                    .Select(ur => ur.Role)
                    .ToListAsync();

                return new ServiceResponse<IEnumerable<RoleModel>>(roles);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<RoleModel>>(ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetUsersByRoleAsync(short roleId)
        {
            try
            {
                var users = await _context.UserRoles
                    .Where(ur => ur.RoleId == roleId)
                    .Include(ur => ur.User)
                    .Select(ur => ur.User)
                    .ToListAsync();

                var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
                return new ServiceResponse<IEnumerable<UserDto>>(userDtos);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<UserDto>>(ex.Message);
            }
        }
    }
}