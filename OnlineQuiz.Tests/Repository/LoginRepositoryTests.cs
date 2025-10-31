using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineQuiz.Configuration;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.Mappings;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.Repository;
using OnlineQuiz.Utilities;
using Xunit;

namespace OnlineQuiz.Tests.Repository
{
    public class LoginRepositoryTests
    {
        private static readonly Lazy<IMapper> CachedMapper = new(() =>
        {
            var loggerFactory = LoggerFactory.Create(builder => { builder.ClearProviders(); builder.SetMinimumLevel(LogLevel.Critical); });
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<AutoMapperProfile>(); }, loggerFactory);
            return config.CreateMapper();
        });

        private static readonly Lazy<IOptions<JwtSettings>> CachedJwtOptions = new(() =>
        {
            var settings = new JwtSettings
            {
                SecretKey = "super_secret_key_12345678901234567890",
                Issuer = "OnlineQuizIssuer",
                Audience = "OnlineQuizAudience",
                AccessTokenExpirationInMinutes = 30,
                RefreshTokenExpirationInDays = 7
            };
            return Options.Create(settings);
        });

        private static readonly ConcurrentDictionary<string, string> HashCache = new();
        private static string LowCostHash(string password)
            => HashCache.GetOrAdd(password, p => BCrypt.Net.BCrypt.HashPassword(p, BCrypt.Net.BCrypt.GenerateSalt(4)));

        private static IMapper CreateMapper()
        {
            return CachedMapper.Value;
        }

        private static IOptions<JwtSettings> CreateJwtOptions()
        {
            return CachedJwtOptions.Value;
        }

        private static OnlineQuizDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new OnlineQuizDbContext(options);
        }

        private static async Task SeedUserWithRoleAsync(OnlineQuizDbContext context, long userId, string email, string fullName, string passwordPlain, string status, short roleId, string roleName)
        {
            var role = new RoleModel { RoleId = roleId, Name = roleName };
            // Use cached low-cost BCrypt for tests to speed up hashing
            var hashed = LowCostHash(passwordPlain);
            var user = new UserModel
            {
                UserId = userId,
                Email = email,
                FullName = fullName,
                PasswordHash = hashed,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Roles.AddAsync(role);
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var userRole = new UserRoleModel { UserId = user.UserId, RoleId = role.RoleId };
            await context.UserRoles.AddAsync(userRole);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsSuccess_WithValidCredentials()
        {
            var dbName = $"LoginRepo_Auth_Success_{Guid.NewGuid()}";
            using var context = CreateContext(dbName);
            await SeedUserWithRoleAsync(context, 1, "john.doe@example.com", "John Doe", "P@ssw0rd!", "Active", 1, "Admin");

            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var repo = new LoginRepository(context, mapper, jwtOptions);

            var loginDto = new LoginDto { Email = "john.doe@example.com", Password = "P@ssw0rd!" };
            var response = await repo.AuthenticateAsync(loginDto);

            Assert.True(response.Success, response.Message);
            Assert.NotNull(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Data!.AccessToken));
            Assert.Equal(jwtOptions.Value.AccessTokenExpirationInMinutes * 60, response.Data.ExpiresIn);
            Assert.NotNull(response.Data.User);
            Assert.Equal("john.doe@example.com", response.Data.User!.Email);
            Assert.Equal("John Doe", response.Data.User!.FullName);
            Assert.Contains("Admin", response.Data.User!.Roles);

            // Validate the token can be parsed with the same settings
            var principal = JwtTokenHelper.ValidateToken(response.Data!.AccessToken!, jwtOptions.Value);
            Assert.NotNull(principal);
        }

        [Fact]
        public async Task AuthenticateAsync_Fails_WithInvalidPassword()
        {
            var dbName = $"LoginRepo_Auth_InvalidPwd_{Guid.NewGuid()}";
            using var context = CreateContext(dbName);
            await SeedUserWithRoleAsync(context, 2, "jane.smith@example.com", "Jane Smith", "CorrectHorseBatteryStaple", "Active", 2, "Student");

            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var repo = new LoginRepository(context, mapper, jwtOptions);

            var loginDto = new LoginDto { Email = "jane.smith@example.com", Password = "wrong-password" };
            var response = await repo.AuthenticateAsync(loginDto);

            Assert.False(response.Success);
            Assert.Null(response.Data);
            Assert.Equal("Invalid email or password", response.Message);
        }

        [Fact]
        public async Task AuthenticateAsync_Fails_WhenUserInactive()
        {
            var dbName = $"LoginRepo_Auth_Inactive_{Guid.NewGuid()}";
            using var context = CreateContext(dbName);
            await SeedUserWithRoleAsync(context, 3, "inactive.user@example.com", "Inactive User", "Secret123", "Suspended", 3, "Teacher");

            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var repo = new LoginRepository(context, mapper, jwtOptions);

            var loginDto = new LoginDto { Email = "inactive.user@example.com", Password = "Secret123" };
            var response = await repo.AuthenticateAsync(loginDto);

            Assert.False(response.Success);
            Assert.Null(response.Data);
            Assert.Equal("User account is not active", response.Message);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_ReturnsSuccess_ForActiveUser()
        {
            var dbName = $"LoginRepo_Validate_Success_{Guid.NewGuid()}";
            using var context = CreateContext(dbName);
            await SeedUserWithRoleAsync(context, 4, "active.user@example.com", "Active User", "MyPassword!", "Active", 2, "Student");

            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var repo = new LoginRepository(context, mapper, jwtOptions);

            var response = await repo.ValidateUserCredentialsAsync("active.user@example.com", "MyPassword!");

            Assert.True(response.Success, response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal("active.user@example.com", response.Data!.Email);
            Assert.Equal("Active", response.Data!.Status);
            Assert.Equal("Credentials validated successfully", response.Message);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_Fails_WithInvalidCredentials()
        {
            var dbName = $"LoginRepo_Validate_Invalid_{Guid.NewGuid()}";
            using var context = CreateContext(dbName);
            await SeedUserWithRoleAsync(context, 5, "validate.user@example.com", "Validate User", "GoodPassword", "Active", 1, "Admin");

            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var repo = new LoginRepository(context, mapper, jwtOptions);

            var response = await repo.ValidateUserCredentialsAsync("validate.user@example.com", "BadPassword");

            Assert.False(response.Success);
            Assert.Null(response.Data);
            Assert.Equal("Invalid credentials", response.Message);
        }

        [Fact]
        public async Task ValidateUserCredentialsAsync_Fails_WhenInactive()
        {
            var dbName = $"LoginRepo_Validate_Inactive_{Guid.NewGuid()}";
            using var context = CreateContext(dbName);
            await SeedUserWithRoleAsync(context, 6, "inactive.validate@example.com", "Inactive Validate", "InactivePwd", "Inactive", 2, "Student");

            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var repo = new LoginRepository(context, mapper, jwtOptions);

            var response = await repo.ValidateUserCredentialsAsync("inactive.validate@example.com", "InactivePwd");

            Assert.False(response.Success);
            Assert.Null(response.Data);
            Assert.Equal("User account is not active", response.Message);
        }

        [Fact]
        public async Task LogoutAsync_ReturnsSuccess_AndUpdatesTimestamp()
        {
            var dbName = $"LoginRepo_Logout_Success_{Guid.NewGuid()}";
            using var context = CreateContext(dbName);
            await SeedUserWithRoleAsync(context, 7, "logout.user@example.com", "Logout User", "LogoutPwd", "Active", 1, "Admin");

            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var repo = new LoginRepository(context, mapper, jwtOptions);

            var before = DateTime.UtcNow.AddSeconds(-1);
            var response = await repo.LogoutAsync(7);

            Assert.True(response.Success, response.Message);
            Assert.Equal("Logout successful", response.Message);

            var user = await context.Users.FindAsync(7L);
            Assert.NotNull(user);
            Assert.True(user!.UpdatedAt >= before);
        }

        [Fact]
        public async Task LogoutAsync_Fails_WhenUserNotFound()
        {
            var dbName = $"LoginRepo_Logout_NotFound_{Guid.NewGuid()}";
            using var context = CreateContext(dbName);
            var mapperStub = new Mock<IMapper>().Object;
            var jwtOptionsStub = Options.Create(new JwtSettings());
            var repo = new LoginRepository(context, mapperStub, jwtOptionsStub);

            var response = await repo.LogoutAsync(9999);
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }
    }
}