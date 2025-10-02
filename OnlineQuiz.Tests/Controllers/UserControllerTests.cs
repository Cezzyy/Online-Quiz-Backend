using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineQuiz.Controllers;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace OnlineQuiz.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);

            // Setup controller context with claims for authorization tests
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResult_WithUsersList()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { UserId = 1, Email = "user1@example.com", FullName = "User One" },
                new UserDto { UserId = 2, Email = "user2@example.com", FullName = "User Two" }
            };

            _mockUserService.Setup(service => service.GetAllUsersAsync())
                .ReturnsAsync(new ServiceResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = users
                });

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
            Assert.True(GetProperty<bool>(okResult.Value!, "success"));
            Assert.Equal("Users retrieved successfully", GetProperty<string>(okResult.Value!, "message"));
            Assert.Equal(2, GetProperty<int>(okResult.Value!, "count"));
        }

        [Fact]
        public async Task GetUserById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var userId = 1L;
            var user = new UserDto { UserId = userId, Email = "user@example.com", FullName = "Test User" };

            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync(new ServiceResponse<UserDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = user
                });

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
            Assert.True(GetProperty<bool>(okResult.Value!, "success"));
            Assert.Equal("User retrieved successfully", GetProperty<string>(okResult.Value!, "message"));
        }

        [Fact]
        public async Task GetUserById_WithInvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0L;

            // Act
            var result = await _controller.GetUserById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetUserById_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 999L;

            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync(new ServiceResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                });

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var createUserDto = new CreateUserDto
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                FullName = "New User",
                Roles = new List<string> { "Student" }
            };

            var createdUser = new UserDto
            {
                UserId = 3,
                Email = "newuser@example.com",
                FullName = "New User"
            };

            _mockUserService.Setup(service => service.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(new ServiceResponse<UserDto>
                {
                    Success = true,
                    Message = "User created successfully",
                    Data = createdUser
                });

            // Act
            var result = await _controller.CreateUser(createUserDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAtActionResult.StatusCode);
            Assert.NotNull(createdAtActionResult.Value);
            Assert.True(GetProperty<bool>(createdAtActionResult.Value!, "success"));
            Assert.Equal("User created successfully", GetProperty<string>(createdAtActionResult.Value!, "message"));
        }

        [Fact]
        public async Task DeleteUser_CannotDeleteSelf_ReturnsBadRequest()
        {
            // Arrange - Current user has ID 1 (from the claims setup)
            var userId = 1L;

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.NotNull(badRequestResult.Value);
            Assert.Equal("You cannot delete your own account", GetProperty<string>(badRequestResult.Value!, "message"));
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