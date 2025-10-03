using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.Utilities;
using System.Security.Claims;

namespace OnlineQuiz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly OnlineQuizDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IActivityLogService _activityLogService;

        public AuthController(IAuthService authService, OnlineQuizDbContext context, ILogger<AuthController> logger, IActivityLogService activityLogService)
        {
            _authService = authService;
            _context = context;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        /// <param name="loginDto">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            // 1) Session Guard: Check if this device already has an active session
            var refreshCookie = Request.Cookies["__Host-refresh"];
            if (!string.IsNullOrEmpty(refreshCookie))
            {
                try
                {
                    var pepper = RefreshTokenHasher.GetPepper();
                    var hash = RefreshTokenHasher.HashToken(refreshCookie, pepper);
                    
                    var activeToken = await _context.RefreshTokens
                        .FirstOrDefaultAsync(t => t.TokenHash == hash && 
                                                 t.RevokedAt == null && 
                                                 t.ExpiresAt > DateTime.UtcNow);

                    if (activeToken != null)
                    {
                        // Check if client explicitly wants to force re-authentication
                        var forceReauth = Request.Headers.ContainsKey("X-Force-Reauth") && 
                                         Request.Headers["X-Force-Reauth"].ToString().ToLower() == "true";
                        
                        if (!forceReauth)
                        {
                            _logger.LogWarning("Login attempt blocked: User already has active session. TokenId: {TokenId}, UserId: {UserId}, IP: {ClientIP}", 
                                activeToken.Id, activeToken.UserId, Request.HttpContext.Connection.RemoteIpAddress);
                            
                            return Problem(
                                title: "Already signed in",
                                statusCode: 409,
                                type: "auth/already_authenticated",
                                detail: "You already have an active session on this device. Use X-Force-Reauth header to force re-authentication.");
                        }
                        
                        _logger.LogInformation("Force re-authentication: Revoking previous session. TokenId: {TokenId}, UserId: {UserId}, IP: {ClientIP}", 
                            activeToken.Id, activeToken.UserId, Request.HttpContext.Connection.RemoteIpAddress);
                        
                        activeToken.RevokedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Session guard error during login attempt. IP: {ClientIP}", 
                        Request.HttpContext.Connection.RemoteIpAddress);
                }
            }

            // 2) Proceed with normal credential check and token issuance
            var response = await _authService.AuthenticateAsync(loginDto);
            if (response == null || !response.Success || response.Data == null)
            {
                _logger.LogWarning("Failed login attempt for email: {Email}, IP: {ClientIP}, Reason: {Reason}", 
                    loginDto.Email, Request.HttpContext.Connection.RemoteIpAddress, response?.Message ?? "Invalid credentials");
                return Unauthorized(response?.Message ?? "Invalid email or password");
            }

            // Check if request is from web client (has specific header or user agent)
            var userAgent = Request.Headers.UserAgent.ToString().ToLower();
            var clientType = Request.Headers["X-Client-Type"].ToString().ToLower();
            
            // Determine if this is a web client
            var isWebClient = clientType == "web" || userAgent.Contains("mozilla");
            
            // Set JWT and refresh token cookies for web clients
            if (isWebClient)
            {
                // Access token cookie (short-lived)
                var accessTokenOptions = new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = true, 
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn),
                    Path = "/" 
                };
                
                // Refresh token cookie (long-lived)
                var refreshTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, 
                    SameSite = SameSiteMode.Lax, 
                    Expires = DateTime.UtcNow.AddDays(7),
                    Path = "/" 
                };
                
                if (!string.IsNullOrEmpty(response.Data.AccessToken))
                {
                    Response.Cookies.Append("__Host-jwt", response.Data.AccessToken, accessTokenOptions);
                }
                if (!string.IsNullOrEmpty(response.Data.RefreshToken))
                {
                    Response.Cookies.Append("__Host-refresh", response.Data.RefreshToken, refreshTokenOptions);
                }
                
                response.Data.AccessToken = null;
                response.Data.RefreshToken = null;
                response.Data.RefreshExpiresIn = null;
            }

            // Log successful login
            _logger.LogInformation("Successful login for user: {Email}, UserId: {UserId}, ClientType: {ClientType}, IP: {ClientIP}", 
                loginDto.Email, response.Data.User?.Id, isWebClient ? "Web" : "Mobile", Request.HttpContext.Connection.RemoteIpAddress);

            // Log activity
            if (response.Data.User?.Id != null)
            {
                await _activityLogService.LogUserActionAsync(response.Data.User.Id, "LOGIN", 
                    $"User logged in successfully from {(isWebClient ? "Web" : "Mobile")} client");
            }

            return Ok(response);
        }

        /// <summary>
        /// Logout user and clear authentication cookies
        /// </summary>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get user information from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                
                // Clear both JWT and refresh token cookies for web clients
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(-1), // Expire the cookie
                    Path = "/"
                };
                
                Response.Cookies.Append("__Host-jwt", "", cookieOptions);
                Response.Cookies.Append("__Host-refresh", "", cookieOptions);
                
                // Revoke refresh tokens in database if user ID is available
                if (long.TryParse(userIdClaim, out var userId))
                {
                    var logoutResult = await _authService.LogoutAsync(userId);
                    if (!logoutResult.Success)
                    {
                        // I should do log error here gamit Ilogger and type shit pero unya lang wait lang
                    }
                    
                    // Log activity
                    await _activityLogService.LogUserActionAsync(userId, "LOGOUT", "User logged out successfully");
                }
                
                return Ok(new { 
                    message = "Logged out successfully",
                    user = new {
                        id = userIdClaim,
                        email
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception)
            {
                // Still clear both cookies even if there's an error
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Path = "/"
                };
                
                Response.Cookies.Append("__Host-jwt", "", cookieOptions);
                Response.Cookies.Append("__Host-refresh", "", cookieOptions);
                
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

        /// <summary>
        /// Refresh access token using refresh token
        /// Supports both web clients (cookie-based) and mobile clients (body-based)
        /// </summary>
        /// <param name="request">Optional refresh token for mobile clients</param>
        /// <returns>New access token and refresh token</returns>
        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshRequest? request = null)
        {
            // 1) Try cookie first (web browsers)
            var cookieToken = Request.Cookies["__Host-refresh"];
            
            // 2) Fallback to body (mobile/API clients)
            var incomingToken = !string.IsNullOrWhiteSpace(cookieToken) 
                ? cookieToken 
                : request?.RefreshToken;
            
            if (string.IsNullOrWhiteSpace(incomingToken))
            {
                return Problem(statusCode: 401, title: "Missing refresh token");
            }

            // Validate and rotate the refresh token
            var tokenDto = new RefreshTokenDto { RefreshToken = incomingToken };
            var response = await _authService.RefreshTokenAsync(tokenDto);
            
            if (response == null || !response.Success || response.Data == null)
            {
                // Clear invalid cookies if token came from cookie (web client)
                if (!string.IsNullOrWhiteSpace(cookieToken))
                {
                    Response.Cookies.Delete("__Host-refresh");
                    Response.Cookies.Delete("__Host-jwt");
                }
                return Problem(statusCode: 401, title: response?.Message ?? "Invalid refresh token");
            }

            // Set new cookies for web clients (if token came from cookie)
            var isWebClient = !string.IsNullOrWhiteSpace(cookieToken);
            if (isWebClient)
            {
                // Access token cookie (short-lived)
                var accessTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddSeconds(response.Data.ExpiresIn),
                    Path = "/"
                };
                
                // Refresh token cookie (long-lived)
                var refreshTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7), // 7 days
                    Path = "/"
                };
                
                if (!string.IsNullOrEmpty(response.Data.AccessToken))
                {
                    Response.Cookies.Append("__Host-jwt", response.Data.AccessToken, accessTokenOptions);
                }
                if (!string.IsNullOrEmpty(response.Data.RefreshToken))
                {
                    Response.Cookies.Append("__Host-refresh", response.Data.RefreshToken, refreshTokenOptions);
                }
                
                response.Data.AccessToken = null;
                response.Data.RefreshToken = null;
                response.Data.RefreshExpiresIn = null;
            }

            return Ok(response);
        }
    }
}