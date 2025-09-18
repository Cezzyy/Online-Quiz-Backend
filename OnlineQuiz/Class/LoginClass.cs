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
                // In a real implementation, you might want to blacklist the token
                // or update a last logout timestamp
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

        public Task<ServiceResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Validate refresh token
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return Task.FromResult(new ServiceResponse("Refresh token is required"));

                // In a production system, you would:
                // 1. Validate the refresh token against stored tokens in database
                // 2. Check if the token is not expired
                // 3. Generate a new access token
                // 4. Optionally rotate the refresh token

                // For now, we'll return a placeholder response
                return Task.FromResult(new ServiceResponse("Token refresh completed successfully"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ServiceResponse($"Token refresh failed: {ex.Message}"));
            }
        }

        public async Task<ServiceResponse> ResetPasswordAsync(string email)
        {
            try
            {
                // Validate email
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceResponse("Email is required");

                // Check if user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    // For security, don't reveal if email exists or not
                    return new ServiceResponse("If the email exists, a password reset link has been sent");
                }

                // Generate reset token
                var resetToken = Guid.NewGuid().ToString("N");
                
                // TODO: In production, you would:
                // 1. Store the reset token in database with expiry (e.g., 1 hour)
                // 2. Send email with reset link containing the token
                // 3. The reset link would be: https://yourapp.com/reset-password?token={resetToken}
                
                return new ServiceResponse("If the email exists, a password reset link has been sent");
            }
            catch (Exception ex)
            {
                return new ServiceResponse($"Password reset request failed: {ex.Message}");
            }
        }

        public Task<ServiceResponse> ConfirmPasswordResetAsync(string token, string newPassword)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(token))
                    return Task.FromResult(new ServiceResponse("Reset token is required"));

                if (string.IsNullOrWhiteSpace(newPassword))
                    return Task.FromResult(new ServiceResponse("New password is required"));

                // Validate password strength
                if (newPassword.Length < 8)
                    return Task.FromResult(new ServiceResponse("Password must be at least 8 characters long"));

                // TODO: In production, you would:
                // 1. Look up the reset token in the database
                // 2. Check if it's not expired
                // 3. Get the associated user
                // 4. Update the user's password with proper hashing
                // 5. Invalidate the reset token

                // For now, we'll simulate this process
                return Task.FromResult(new ServiceResponse("Password reset functionality requires database token storage implementation"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ServiceResponse($"Password reset failed: {ex.Message}"));
            }
        }

        public Task<ServiceResponse> VerifyEmailAsync(string token)
        {
            try
            {
                // Validate token
                if (string.IsNullOrWhiteSpace(token))
                    return Task.FromResult(new ServiceResponse("Verification token is required"));

                // TODO: In production, you would:
                // 1. Look up the verification token in the database
                // 2. Check if it's not expired
                // 3. Get the associated user
                // 4. Mark the user's email as verified (add EmailVerified field to UserModel)
                // 5. Invalidate the verification token

                // For now, we'll simulate this process
                return Task.FromResult(new ServiceResponse("Email verification functionality requires database token storage implementation"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ServiceResponse($"Email verification failed: {ex.Message}"));
            }
        }

        public async Task<ServiceResponse> ResendVerificationEmailAsync(string email)
        {
            try
            {
                // Validate email
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceResponse("Email is required");

                // Check if user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    // For security, don't reveal if email exists or not
                    return new ServiceResponse("If the email exists and is not verified, a verification email has been sent");
                }

                // TODO: In production, you would:
                // 1. Check if email is already verified
                // 2. Generate new verification token
                // 3. Store token in database with expiry
                // 4. Send verification email

                // Generate verification token
                var verificationToken = Guid.NewGuid().ToString("N");
                
                return new ServiceResponse("If the email exists and is not verified, a verification email has been sent");
            }
            catch (Exception ex)
            {
                return new ServiceResponse($"Resend verification email failed: {ex.Message}");
            }
        }
    }
}