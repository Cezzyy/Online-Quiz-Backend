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
                new UserDto { UserId = 1, Email = "user1@example.com", FirstName = "User", LastName = "One" },
                new UserDto { UserId = 2, Email = "user2@example.com", FirstName = "User", LastName = "Two" }
            };

            _mockUserService.Setup(service => service.GetAllUsersAsync())
                .ReturnsAsync(new ApiResponse<IEnumerable<UserDto>>
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
            
            dynamic response = okResult.Value;
            Assert.True(response.success);
            Assert.Equal("Users retrieved successfully", response.message);
            Assert.Equal(2, response.count);
        }

        [Fact]
        public async Task GetUserById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var userId = 1L;
            var user = new UserDto { UserId = userId, Email = "user@example.com", FirstName = "Test", LastName = "User" };

            _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
                .ReturnsAsync(new ApiResponse<UserDto>
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
            
            dynamic response = okResult.Value;
            Assert.True(response.success);
            Assert.Equal("User retrieved successfully", response.message);
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
                .ReturnsAsync(new ApiResponse<UserDto>
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
                FirstName = "New",
                LastName = "User",
                RoleName = "Student"
            };

            var createdUser = new UserDto
            {
                UserId = 3,
                Email = "newuser@example.com",
                FirstName = "New",
                LastName = "User"
            };

            _mockUserService.Setup(service => service.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(new ApiResponse<UserDto>
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
            
            dynamic response = createdAtActionResult.Value;
            Assert.True(response.success);
            Assert.Equal("User created successfully", response.message);
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
            
            dynamic response = badRequestResult.Value;
            Assert.Equal("You cannot delete your own account", response.message);
        }
    }
}