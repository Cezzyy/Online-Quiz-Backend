using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineQuiz.Controllers;
using OnlineQuiz.Data;
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
        private readonly OnlineQuizDbContext _dbContext;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly Mock<IActivityLogService> _activityLogServiceMock;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<OnlineQuiz.IServices.IAuthService>();

            // Set up in-memory DbContext and logger for controller constructor
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase(databaseName: "AuthControllerTestsDb")
                .Options;
            _dbContext = new OnlineQuizDbContext(options);
            _loggerMock = new Mock<ILogger<AuthController>>();
            _activityLogServiceMock = new Mock<IActivityLogService>();

            // Ensure awaited logging calls return a non-null Task
            _activityLogServiceMock
                .Setup(s => s.LogUserActionAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _activityLogServiceMock
                .Setup(s => s.LogEntityActionAsync(
                    It.IsAny<long>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<object?>()))
                .Returns(Task.CompletedTask);

            _controller = new AuthController(_mockAuthService.Object, _dbContext, _loggerMock.Object, _activityLogServiceMock.Object);
            
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
                User = new UserSummaryDto { Id = 1, Email = "test@example.com", FullName = "Test User", Roles = new List<string>() }
            };

            var apiResponse = new ServiceResponse<LoginResponseDto>
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
            var returnValue = Assert.IsType<ServiceResponse<LoginResponseDto>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(responseData, returnValue.Data);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "invalid@example.com", Password = "wrongpassword" };
            var apiResponse = new ServiceResponse<LoginResponseDto>
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
        public async Task Logout_AuthenticatedUser_ReturnsOkResult()
        {
            // Arrange
            SetupUserClaims("1", "test@example.com", "Test User");

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Equal("Logged out successfully", GetProperty<string>(okResult.Value!, "message"));
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
            Assert.NotNull(okResult.Value);
            Assert.True(GetProperty<bool>(okResult.Value!, "valid"));
            Assert.Equal("1", GetProperty<string>(okResult.Value!, "user.id"));
            Assert.Equal("test@example.com", GetProperty<string>(okResult.Value!, "user.email"));
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

            var apiResponse = new ServiceResponse<RefreshTokenResponseDto>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = responseData
            };

            _mockAuthService.Setup(s => s.RefreshTokenAsync(It.IsAny<OnlineQuiz.DTOs.RefreshTokenDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.RefreshToken(refreshRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<ServiceResponse<RefreshTokenResponseDto>>(okResult.Value);
            Assert.True(returnValue.Success);
            Assert.Equal(responseData, returnValue.Data);
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_ReturnsProblem()
        {
            // Arrange
            var refreshRequest = new OnlineQuiz.DTOs.RefreshRequest { RefreshToken = "invalid-refresh-token" };
            var apiResponse = new ServiceResponse<RefreshTokenResponseDto>
            {
                Success = false,
                Message = "Invalid refresh token",
                Data = null
            };

            _mockAuthService.Setup(s => s.RefreshTokenAsync(It.IsAny<OnlineQuiz.DTOs.RefreshTokenDto>()))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _controller.RefreshToken(refreshRequest);

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

        private static T? GetProperty<T>(object obj, string propertyPath)
        {
            object? current = obj;
            foreach (var name in propertyPath.Split('.'))
            {
                if (current == null) return default;
                var type = current.GetType();
                var prop = type.GetProperty(name);
                if (prop == null) return default;
                current = prop.GetValue(current);
            }
            return current is T t ? t : default;
        }
    }
}