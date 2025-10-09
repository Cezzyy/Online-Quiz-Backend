using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using OnlineQuiz.Configuration;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.Services;
using Xunit;

namespace OnlineQuiz.Tests.Services
{
    public class AuthServiceTests
    {
        public AuthServiceTests()
        {
            // Ensure pepper is set for refresh token hashing/verification
            // Use a valid Base64 string for pepper
            Environment.SetEnvironmentVariable("REFRESH_TOKEN_PEPPER", "dGVzdC1wZXBwZXI=");
        }
        private OnlineQuizDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase($"AuthServiceTests_{Guid.NewGuid()}")
                .Options;
            return new OnlineQuizDbContext(options);
        }

        private IConfiguration CreateConfig()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"JwtSettings:SecretKey", "a-very-long-secret-key-for-tests-1234567890"},
                {"JwtSettings:AccessTokenExpirationInMinutes", "15"},
                {"JwtSettings:RefreshTokenExpirationInDays", "7"},
            };
            return new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        }

        private UserModel SeedUser(OnlineQuizDbContext context)
        {
            var role = new RoleModel { RoleId = 1, Name = "Admin" };
            var user = new UserModel
            {
                UserId = 100,
                Email = "user@example.com",
                FullName = "Test User",
                PasswordHash = OnlineQuiz.Utilities.PasswordHelper.HashPassword("password"),
                Status = "Active",
                UserRoles = new List<UserRoleModel> { new UserRoleModel { Role = role, RoleId = role.RoleId, UserId = 100 } }
            };
            context.Roles.Add(role);
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsTokensAndUserSummary_OnValidCredentials()
        {
            // Arrange
            var context = CreateDbContext();
            var configuration = CreateConfig();
            var user = SeedUser(context);

            var loginRepoMock = new Mock<ILoginRepository>();
            var loginDto = new LoginDto { Email = user.Email!, Password = "password" };
            var loginResponse = new LoginResponseDto
            {
                AccessToken = "dummy",
                User = new UserSummaryDto { Id = user.UserId, Email = user.Email!, FullName = user.FullName!, Roles = new List<string> { "Admin" } },
            };
            loginRepoMock.Setup(r => r.AuthenticateAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(new ServiceResponse<LoginResponseDto>
                {
                    Success = true,
                    Data = loginResponse,
                    Message = "OK"
                });

            var service = new AuthService(loginRepoMock.Object, configuration, context);

            // Act
            var result = await service.AuthenticateAsync(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.False(string.IsNullOrEmpty(result.Data!.AccessToken));
            Assert.NotNull(result.Data.RefreshToken);
            Assert.Equal("Bearer", result.Data.TokenType);
            Assert.True(result.Data.ExpiresIn > 0);
            Assert.True(result.Data.RefreshExpiresIn > 0);

            // Verify refresh token stored hashed
            var stored = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == user.UserId);
            Assert.NotNull(stored);
            Assert.NotEqual(result.Data.RefreshToken, stored!.TokenHash);
        }

        [Fact]
        public async Task AuthenticateAsync_InvalidInput_ReturnsError()
        {
            var context = CreateDbContext();
            var configuration = CreateConfig();
            var loginRepoMock = new Mock<ILoginRepository>();

            var service = new AuthService(loginRepoMock.Object, configuration, context);

            var result = await service.AuthenticateAsync(new LoginDto { Email = "", Password = "" });
            Assert.False(result.Success);
            Assert.Equal("Email is required", result.Message);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_DelegatesToRepository()
        {
            var context = CreateDbContext();
            var configuration = CreateConfig();
            var loginRepoMock = new Mock<ILoginRepository>();
            loginRepoMock.Setup(r => r.ValidateUserCredentialsAsync("e@x.com", "p"))
                .ReturnsAsync(new ServiceResponse<UserModel> { Success = true, Data = new UserModel { UserId = 1 } });

            var service = new AuthService(loginRepoMock.Object, configuration, context);
            var result = await service.ValidateUserCredentialsAsync("e@x.com", "p");
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task RefreshTokenAsync_InvalidToken_ReturnsError()
        {
            var context = CreateDbContext();
            var configuration = CreateConfig();
            var loginRepoMock = new Mock<ILoginRepository>();
            var service = new AuthService(loginRepoMock.Object, configuration, context);

            var res = await service.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = "invalid" });
            Assert.False(res.Success);
            Assert.Equal("Invalid or expired refresh token", res.Message);
        }
    }
}