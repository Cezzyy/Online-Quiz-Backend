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
    public class LoginRepository : ILoginRepository
    {
        private readonly OnlineQuizDbContext _context;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public LoginRepository(OnlineQuizDbContext context, IMapper mapper, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _mapper = mapper;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ServiceResponse<LoginResponseDto>> AuthenticateAsync(LoginDto loginDto)
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
                    Message = "Authentication successful"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = $"Error during authentication: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserModel>> ValidateUserCredentialsAsync(string email, string password)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
                {
                    return new ServiceResponse<UserModel>
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                if (user.Status != "Active")
                {
                    return new ServiceResponse<UserModel>
                    {
                        Success = false,
                        Message = "User account is not active"
                    };
                }

                return new ServiceResponse<UserModel>
                {
                    Success = true,
                    Data = user,
                    Message = "Credentials validated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserModel>
                {
                    Success = false,
                    Message = $"Error validating credentials: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse> LogoutAsync(long userId)
        {
            try
            {
                // In a stateless JWT system, logout is typically handled client-side
                // However, you could implement token blacklisting here if needed
                
                // For now, we'll just return success
                // In a production system, you might want to:
                // 1. Add the token to a blacklist
                // 2. Update user's last logout time
                // 3. Invalidate refresh tokens

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Update last activity or logout time if you have such fields
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ServiceResponse
                {
                    Success = true,
                    Message = "Logout successful"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = $"Error during logout: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // This is a placeholder implementation
                // In a production system, you would:
                // 1. Validate the refresh token
                // 2. Check if it's not expired
                // 3. Generate a new access token
                // 4. Optionally rotate the refresh token

                // For now, return not implemented
                return new ServiceResponse
                {
                    Success = false,
                    Message = "Refresh token functionality not implemented yet"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse
                {
                    Success = false,
                    Message = $"Error refreshing token: {ex.Message}"
                };
            }
        }


    }
}