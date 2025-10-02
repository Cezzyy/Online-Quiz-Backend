using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineQuiz.Class;
using OnlineQuiz.Configuration;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.Mappings;
using OnlineQuiz.Models;
using OnlineQuiz.Utilities;
using Xunit;

namespace OnlineQuiz.Tests.Class
{
    public class LoginClassTests
    {
        private static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            return config.CreateMapper();
        }

        private static IOptions<JwtSettings> CreateJwtOptions()
        {
            return Options.Create(new JwtSettings
            {
                SecretKey = "UnitTestSecretKey1234567890",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                AccessTokenExpirationInMinutes = 5
            });
        }

        private static OnlineQuizDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new OnlineQuizDbContext(options);
        }

        private static UserModel CreateUser(string email, string password, string fullName = "Test User")
        {
            return new UserModel
            {
                Email = email,
                FullName = fullName,
                PasswordHash = PasswordHelper.HashPassword(password),
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsTokenAndUser_OnValidCredentials()
        {
            var db = CreateDbContext(nameof(AuthenticateAsync_ReturnsTokenAndUser_OnValidCredentials));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var user = CreateUser("user@example.com", "P@ssw0rd", "Alice Tester");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new LoginClass(db, mapper, jwtOptions);

            var response = await sut.AuthenticateAsync(new LoginDto
            {
                Email = "user@example.com",
                Password = "P@ssw0rd"
            });

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Data!.AccessToken));
            Assert.Equal(jwtOptions.Value.AccessTokenExpirationInMinutes * 60, response.Data.ExpiresIn);
            Assert.NotNull(response.Data.User);
            Assert.Equal(user.Email, response.Data.User!.Email);
            Assert.Equal(user.FullName, response.Data.User.FullName);
        }

        [Fact]
        public async Task AuthenticateAsync_Fails_ForInvalidPassword()
        {
            var db = CreateDbContext(nameof(AuthenticateAsync_Fails_ForInvalidPassword));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var user = CreateUser("user@example.com", "CorrectPassword");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new LoginClass(db, mapper, jwtOptions);
            var response = await sut.AuthenticateAsync(new LoginDto
            {
                Email = "user@example.com",
                Password = "WrongPassword"
            });

            Assert.False(response.Success);
            Assert.Equal("Invalid email or password", response.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_Fails_WhenUserInactive()
        {
            var db = CreateDbContext(nameof(AuthenticateAsync_Fails_WhenUserInactive));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var user = CreateUser("inactive@example.com", "P@ssw0rd");
            user.Status = "Inactive";
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new LoginClass(db, mapper, jwtOptions);
            var response = await sut.AuthenticateAsync(new LoginDto
            {
                Email = "inactive@example.com",
                Password = "P@ssw0rd"
            });

            Assert.False(response.Success);
            Assert.Equal("Account is not active", response.Message);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_ReturnsUser_OnValidCredentials()
        {
            var db = CreateDbContext(nameof(ValidateUserCredentialsAsync_ReturnsUser_OnValidCredentials));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var user = CreateUser("valid@example.com", "ValidPass");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new LoginClass(db, mapper, jwtOptions);
            var response = await sut.ValidateUserCredentialsAsync("valid@example.com", "ValidPass");

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(user.Email, response.Data!.Email);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_Fails_OnInvalidPassword()
        {
            var db = CreateDbContext(nameof(ValidateUserCredentialsAsync_Fails_OnInvalidPassword));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var user = CreateUser("valid@example.com", "ValidPass");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new LoginClass(db, mapper, jwtOptions);
            var response = await sut.ValidateUserCredentialsAsync("valid@example.com", "WrongPass");

            Assert.False(response.Success);
            Assert.Equal("Invalid password", response.Message);
        }

        [Fact]
        public async Task LogoutAsync_UpdatesTimestamp_WhenUserExists()
        {
            var db = CreateDbContext(nameof(LogoutAsync_UpdatesTimestamp_WhenUserExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var user = CreateUser("logout@example.com", "P@ssw0rd");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new LoginClass(db, mapper, jwtOptions);

            var before = user.UpdatedAt;
            var response = await sut.LogoutAsync(user.UserId);
            Assert.True(response.Success);
            Assert.True(user.UpdatedAt >= before);
        }

        [Fact]
        public async Task LogoutAsync_Fails_WhenUserNotFound()
        {
            var db = CreateDbContext(nameof(LogoutAsync_Fails_WhenUserNotFound));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var sut = new LoginClass(db, mapper, jwtOptions);
            var response = await sut.LogoutAsync(12345);

            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }
    }
}