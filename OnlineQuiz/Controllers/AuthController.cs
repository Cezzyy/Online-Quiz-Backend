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
        private readonly IAuthService _authService;

        public AuthController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            var response = await _authService.AuthenticateAsync(loginDto);
            if (response == null || !response.Success || response.Data == null)
            {
                return Unauthorized(response?.Message ?? "Invalid email or password");
            }

            // Check if request is from web client (has specific header or user agent)
            var userAgent = Request.Headers.UserAgent.ToString().ToLower();
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
                        email
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception)
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
                        email,
                        name,
                        roles
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



    }
}