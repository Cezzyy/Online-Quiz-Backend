using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineQuiz.Controllers;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;
using System.Security.Claims;
using Xunit;

// Use the original project's interfaces and DTOs directly

namespace OnlineQuiz.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<OnlineQuiz.IServices.IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<OnlineQuiz.IServices.IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
            
            // Setup default HttpContext for controller
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password123" };
            var responseData = new LoginResponseDto
            {
                AccessToken = "test-token",
                RefreshToken = "refresh-token",
                ExpiresIn = 3600,
                User = new UserInfoDto { Id = "1", Email = "test@example.com", Name = "Test User" }
            };

            var apiResponse = new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = responseData
            };

            _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<OnlineQuiz.DTOs.LoginDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Login(new OnlineQuiz.DTOs.LoginDto { Email = loginDto.Email, Password = loginDto.Password });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<LoginResponseDto>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(responseData, returnValue.Data);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "invalid@example.com", Password = "wrongpassword" };
            var apiResponse = new ApiResponse<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid email or password",
                Data = null
            };

            _mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<OnlineQuiz.DTOs.LoginDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.Login(new OnlineQuiz.DTOs.LoginDto { Email = loginDto.Email, Password = loginDto.Password });

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public void Logout_AuthenticatedUser_ReturnsOkResult()
        {
            // Arrange
            SetupUserClaims("1", "test@example.com", "Test User");

            // Act
            var result = _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value;
            Assert.Equal("Logged out successfully", response.message.ToString());
        }

        [Fact]
        public void VerifyToken_ValidToken_ReturnsUserInfo()
        {
            // Arrange
            SetupUserClaims("1", "test@example.com", "Test User", new[] { "User", "Admin" });

            // Act
            var result = _controller.VerifyToken();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic response = okResult.Value;
            Assert.True(response.valid);
            Assert.Equal("1", response.user.id.ToString());
            Assert.Equal("test@example.com", response.user.email.ToString());
        }

        [Fact]
        public async Task RefreshToken_ValidToken_ReturnsNewTokens()
        {
            // Arrange
            var refreshRequest = new OnlineQuiz.DTOs.RefreshRequest { RefreshToken = "valid-refresh-token" };
            var responseData = new RefreshTokenResponseDto
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token",
                ExpiresIn = 3600
            };

            var apiResponse = new ApiResponse<RefreshTokenResponseDto>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = responseData
            };

            _mockAuthService.Setup(s => s.RefreshTokenAsync(It.IsAny<OnlineQuiz.DTOs.RefreshTokenDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.RefreshToken(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ApiResponse<RefreshTokenResponseDto>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(responseData, returnValue.Data);
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_ReturnsProblem()
        {
            // Arrange
            var refreshRequest = new OnlineQuiz.DTOs.RefreshRequest { RefreshToken = "invalid-refresh-token" };
            var apiResponse = new ApiResponse<RefreshTokenResponseDto>
            {
                Success = false,
                Message = "Invalid refresh token",
                Data = null
            };

            _mockAuthService.Setup(s => s.RefreshTokenAsync(It.IsAny<OnlineQuiz.DTOs.RefreshTokenDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.RefreshToken(null);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(401, problemResult.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_MissingToken_ReturnsProblem()
        {
            // Arrange - passing null for the request
            
            // Act
            var result = await _controller.RefreshToken(null);

            // Assert
            var problemResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(401, problemResult.StatusCode);
        }

        private void SetupUserClaims(string userId, string email, string name, string[]? roles = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString())
            };

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}