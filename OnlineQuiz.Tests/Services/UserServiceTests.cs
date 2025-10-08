using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;
using OnlineQuiz.Services;
using Xunit;

namespace OnlineQuiz.Tests.Services
{
    public class UserServiceTests
    {
        private OnlineQuizDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase($"UserServiceTests_{Guid.NewGuid()}")
                .Options;
            return new OnlineQuizDbContext(options);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsFromRepository()
        {
            var context = CreateDbContext();
            var repoMock = new Mock<IUserRepository>();
            var authMock = new Mock<IAuthService>();
            repoMock.Setup(r => r.GetAllUsersAsync())
                .ReturnsAsync(new ServiceResponse<IEnumerable<UserDto>> { Success = true, Data = new List<UserDto> { new UserDto { UserId = 1, Email = "a@b.com" } } });

            var service = new UserService(repoMock.Object, authMock.Object, context);
            var res = await service.GetAllUsersAsync();

            Assert.True(res.Success);
            Assert.NotNull(res.Data);
        }

        [Fact]
        public async Task CreateUserAsync_DelegatesToRepository()
        {
            var context = CreateDbContext();
            var repoMock = new Mock<IUserRepository>();
            var authMock = new Mock<IAuthService>();

            // 👇 Mock GetUserByEmailAsync so it returns "no existing user"
            repoMock.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ServiceResponse<UserDto> { Success = false, Data = null });

            // 👇 Mock CreateUserAsync to return success
            repoMock.Setup(r => r.CreateUserAsync(It.IsAny<CreateUserDto>()))
                .ReturnsAsync(new ServiceResponse<UserDto>
                {
                    Success = true,
                    Data = new UserDto { UserId = 2, Email = "c@d.com" }
                });

            var service = new UserService(repoMock.Object, authMock.Object, context);

            var res = await service.CreateUserAsync(new CreateUserDto
            {
                Email = "c@d.com",
                Password = "P@ssw0rd",
                FullName = "Test User",
                Roles = new List<string> { "Student" } // required by validation
            });

            Assert.True(res.Success);
            Assert.NotNull(res.Data);
            Assert.Equal(2, res.Data!.UserId);
        }



       [Fact]
       public async Task DeleteUserAsync_RemovesUserFromDatabase()
       {
           var context = CreateDbContext();
           var repoMock = new Mock<IUserRepository>();
           var authMock = new Mock<IAuthService>();

            // Arrange: Mock GetUserByIdAsync to return a valid user
           repoMock.Setup(r => r.GetUserByIdAsync(5))
               .ReturnsAsync(new ServiceResponse<UserDto>
               {
                   Success = true,
                   Data = new UserDto { UserId = 5, Email = "test@delete.com" }
               });

            // Mock DeleteUserAsync to return a successful ServiceResponse with a positive message
            repoMock.Setup(r => r.DeleteUserAsync(5))
                .ReturnsAsync(new ServiceResponse { Success = true, Message = "User deleted successfully" });

            var service = new UserService(repoMock.Object, authMock.Object, context);

            // Act
            var res = await service.DeleteUserAsync(5);

            // Assert
            Assert.True(res.Success);
            Assert.Equal("User deleted successfully", res.Message);
        }
    }
}