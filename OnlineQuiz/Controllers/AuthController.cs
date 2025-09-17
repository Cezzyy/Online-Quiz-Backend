using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using System.Security.Claims;

namespace OnlineQuiz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            var response = await _userService.AuthenticateAsync(loginDto);
            if (response == null)
            {
                return Unauthorized("Invalid email or password");
            }

            // Check if request is from web client (has specific header or user agent)
            var userAgent = Request.Headers["User-Agent"].ToString().ToLower();
            var clientType = Request.Headers["X-Client-Type"].ToString().ToLower();
            
            // Set JWT cookie for web clients (Vue/TypeScript)
            if (clientType == "web" || userAgent.Contains("mozilla"))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Prevent XSS attacks
                    Secure = Request.IsHttps, // Only send over HTTPS in production
                    SameSite = SameSiteMode.Lax, // CSRF protection
                    Expires = DateTime.UtcNow.AddHours(24), // Match JWT expiration
                    Path = "/"
                };
                
                Response.Cookies.Append("jwt", response.Data.Token, cookieOptions);
            }

            return Ok(response);
        }

        /// <summary>
        /// Logout user and clear authentication cookies
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                // Get user information from JWT token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                
                // Clear JWT cookie for web clients
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(-1), // Expire the cookie
                    Path = "/"
                };
                
                Response.Cookies.Append("jwt", "", cookieOptions);
                
                return Ok(new { 
                    message = "Logged out successfully",
                    user = new {
                        id = userId,
                        email = email
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                // Still clear the cookie even if there's an error
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Path = "/"
                };
                
                Response.Cookies.Append("jwt", "", cookieOptions);
                
                return Ok(new { 
                    message = "Logged out successfully",
                    note = "Session cleared",
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="changePasswordDto">Current and new password</param>
        /// <returns>Success message</returns>
        [HttpPost("change-password/{userId}")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(long userId, [FromBody] ChangePasswordDto changePasswordDto)
        {
            var result = await _userService.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(new { message = "Password changed successfully" });
        }



        /// <summary>
        /// Verify JWT token and return user information
        /// </summary>
        /// <returns>Current user information from JWT token</returns>
        [HttpGet("verify")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            try
            {
                // Extract user information from JWT claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var name = User.FindFirst(ClaimTypes.Name)?.Value;
                var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Invalid token: User ID not found" });
                }

                return Ok(new 
                {
                    valid = true,
                    user = new 
                    {
                        id = userId,
                        email = email,
                        name = name,
                        roles = roles
                    },
                    tokenExpiry = User.FindFirst("exp")?.Value,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { 
                    valid = false, 
                    message = "Token verification failed",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Refresh JWT token (if user has valid token)
        /// </summary>
        /// <returns>New JWT token</returns>
        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new { message = "Invalid token: User information not found" });
                }

                // Get user by ID to verify user still exists
                var userResponse = await _userService.GetUserByIdAsync(long.Parse(userId));
                if (!userResponse.Success || userResponse.Data == null)
                {
                    return Unauthorized(new { message = "User not found or inactive" });
                }

                // Since the user is already authenticated (token is valid), 
                // we can generate a new token without password verification
                // This would typically involve calling a token generation service directly
                // For now, we'll return the current token info with extended expiry
                
                return Ok(new 
                {
                    message = "Token is still valid",
                    user = new 
                    {
                        id = userId,
                        email = email,
                        name = User.FindFirst(ClaimTypes.Name)?.Value,
                        roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                    },
                    tokenExpiry = User.FindFirst("exp")?.Value,
                    timestamp = DateTime.UtcNow,
                    note = "Token refresh functionality requires additional token generation service implementation"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Token refresh failed",
                    error = ex.Message 
                });
            }
        }
    }

    /// <summary>
    /// DTO for changing password
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// Current password
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;
        
        /// <summary>
        /// New password
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
    }
}