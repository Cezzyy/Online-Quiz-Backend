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
    public class LoginClass : ILoginRepository
    {
        private readonly OnlineQuizDbContext _context;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public LoginClass(OnlineQuizDbContext context, IMapper mapper, IOptions<JwtSettings> jwtSettings)
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

                if (user == null)
                    return new ServiceResponse<LoginResponseDto>("Invalid email or password");

                if (!PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash))
                    return new ServiceResponse<LoginResponseDto>("Invalid email or password");

                if (user.Status != "Active")
                    return new ServiceResponse<LoginResponseDto>("Account is not active");

                // Get user roles
                var roles = user.UserRoles?.Select(ur => ur.Role.Name) ?? [];
                
                var token = JwtTokenHelper.GenerateToken(user, roles, _jwtSettings);
                var userDto = _mapper.Map<UserDto>(user);
                var userSummary = new UserSummaryDto
                {
                    Id = userDto.UserId,
                    Email = userDto.Email,
                    FullName = userDto.FullName,
                    Roles = [.. roles]
                };
                
                var loginResponse = new LoginResponseDto
                {
                    AccessToken = token,
                    ExpiresIn = _jwtSettings.AccessTokenExpirationInMinutes * 60, // Convert to seconds
                    User = userSummary
                };

                return new ServiceResponse<LoginResponseDto>(loginResponse);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>(ex.Message);
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

                if (user == null)
                    return new ServiceResponse<UserModel>("User not found");

                if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                    return new ServiceResponse<UserModel>("Invalid password");

                if (user.Status != "Active")
                    return new ServiceResponse<UserModel>("Account is not active");

                return new ServiceResponse<UserModel>(user);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserModel>(ex.Message);
            }
        }

        public async Task<ServiceResponse> LogoutAsync(long userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return new ServiceResponse("User not found");

                // Update last activity or logout timestamp if needed
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ServiceResponse();
            }
            catch (Exception ex)
            {
                return new ServiceResponse(ex.Message);
            }
        }
    }
}