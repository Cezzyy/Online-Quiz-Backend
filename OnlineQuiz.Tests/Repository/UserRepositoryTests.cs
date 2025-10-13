using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineQuiz.Configuration;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.Mappings;
using OnlineQuiz.Models;
using OnlineQuiz.Repository;
using Xunit;

namespace OnlineQuiz.Tests.Repository
{
    public class UserRepositoryTests
    {
        private static readonly Lazy<IMapper> CachedMapper = new(() =>
        {
            var loggerFactory = LoggerFactory.Create(builder => { builder.ClearProviders(); builder.SetMinimumLevel(LogLevel.Critical); });
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<AutoMapperProfile>(); }, loggerFactory);
            return config.CreateMapper();
        });

        private static readonly Lazy<IOptions<JwtSettings>> CachedJwtOptions = new(() =>
        {
            return Options.Create(new JwtSettings
            {
                SecretKey = "super-secret-key-for-tests-1234567890-0987654321",
                Issuer = "OnlineQuizAPI",
                Audience = "OnlineQuizUsers",
                AccessTokenExpirationInMinutes = 10,
                RefreshTokenExpirationInDays = 7
            });
        });

        private static readonly ConcurrentDictionary<string, string> HashCache = new();
        private static string LowCostHash(string password)
            => HashCache.GetOrAdd(password, p => BCrypt.Net.BCrypt.HashPassword(p, BCrypt.Net.BCrypt.GenerateSalt(4)));

        private static OnlineQuizDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase($"UserRepositoryTests_{Guid.NewGuid()}")
                .Options;
            return new OnlineQuizDbContext(options);
        }

        private static IMapper CreateMapper()
        {
            return CachedMapper.Value;
        }

        private static IOptions<JwtSettings> CreateJwtOptions()
        {
            return CachedJwtOptions.Value;
        }

        private static RoleModel SeedRole(OnlineQuizDbContext context, string name, short id)
        {
            var role = new RoleModel { RoleId = id, Name = name };
            context.Roles.Add(role);
            context.SaveChanges();
            return role;
        }

        private static UserModel SeedUser(
            OnlineQuizDbContext context,
            string email,
            string plainPassword,
            string fullName,
            string status,
            params string[] roles)
        {
            var user = new UserModel
            {
                Email = email,
                // Use cached low-cost BCrypt for tests to avoid slow hashing
                PasswordHash = LowCostHash(plainPassword),
                FullName = fullName,
                Status = status
            };
            context.Users.Add(user);
            context.SaveChanges();

            var roleEntities = context.Roles.Where(r => roles.Contains(r.Name)).ToList();
            foreach (var role in roleEntities)
            {
                context.UserRoles.Add(new UserRoleModel
                {
                    UserId = user.UserId,
                    RoleId = role.RoleId,
                    Role = role,
                    User = user
                });
            }

            if (roles.Contains("Teacher"))
            {
                context.Teachers.Add(new TeacherModel
                {
                    UserId = user.UserId,
                    Department = "Computer Science",
                    User = user
                });
            }

            if (roles.Contains("Student"))
            {
                context.Students.Add(new StudentModel
                {
                    UserId = user.UserId,
                    StudentNumber = "SN123",
                    YearLevel = 1,
                    Section = "A",
                    Course = "CS",
                    User = user
                });
            }

            context.SaveChanges();
            return user;
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUserDto_WhenExists()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            // Seed roles and user
            SeedRole(context, "Admin", 1);
            SeedRole(context, "Teacher", 2);
            var user = SeedUser(context, "user1@example.com", "Password!", "User One", "Active", "Admin", "Teacher");

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.GetUserByIdAsync(user.UserId);

            Assert.True(res.Success);
            Assert.NotNull(res.Data);
            Assert.Equal(user.Email, res.Data!.Email);
            Assert.Contains("Admin", res.Data.Roles);
            Assert.Contains("Teacher", res.Data.Roles);
        }

        [Fact]
        public async Task GetUserByIdAsync_NotFound_ReturnsError()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.GetUserByIdAsync(99999);

            Assert.False(res.Success);
            Assert.Equal("User not found", res.Message);
            Assert.Null(res.Data);
        }

        [Fact]
        public async Task CreateUserAsync_CreatesUser_AndAssignsTeacherRole()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            // Seed roles used by repository
            SeedRole(context, "Teacher", 2);

            var repo = new UserRepository(context, mapper, jwtOptions);
            var dto = new CreateUserDto
            {
                Email = "newteacher@example.com",
                Password = "P@ss!",
                FullName = "New Teacher",
                Roles = new List<string> { "Teacher" },
                Department = "Mathematics"
            };

            var res = await repo.CreateUserAsync(dto);
            Assert.True(res.Success);
            Assert.NotNull(res.Data);
            Assert.Equal("newteacher@example.com", res.Data!.Email);
            Assert.Contains("Teacher", res.Data.Roles);

            // Verify persisted user and teacher profile
            var persisted = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            Assert.NotNull(persisted);
            Assert.NotEqual(dto.Password, persisted!.PasswordHash);
            var teacher = await context.Teachers.FirstOrDefaultAsync(t => t.UserId == persisted.UserId);
            Assert.NotNull(teacher);
            Assert.Equal("Mathematics", teacher!.Department);
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateEmail_ReturnsFailure()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            SeedRole(context, "Admin", 1);
            // Seed existing user
            SeedUser(context, "dup@example.com", "Password!", "Dup User", "Active", "Admin");

            var repo = new UserRepository(context, mapper, jwtOptions);
            var dto = new CreateUserDto
            {
                Email = "dup@example.com",
                Password = "secret",
                FullName = "Someone",
                Roles = new List<string> { "Admin" }
            };

            var res = await repo.CreateUserAsync(dto);
            Assert.False(res.Success);
            Assert.Equal("User with this email already exists", res.Message);
            Assert.Null(res.Data);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsTokenAndSummary()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            SeedRole(context, "Teacher", 2);
            var user = SeedUser(context, "login@example.com", "P@ssw0rd", "Login User", "Active", "Teacher");

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.LoginAsync(new LoginDto { Email = user.Email, Password = "P@ssw0rd" });

            Assert.True(res.Success);
            Assert.NotNull(res.Data);
            Assert.False(string.IsNullOrWhiteSpace(res.Data!.AccessToken));
            Assert.Equal("Bearer", res.Data.TokenType);
            Assert.True(res.Data.ExpiresIn > 0);
            Assert.Equal(user.Email, res.Data.User.Email);
            Assert.Contains("Teacher", res.Data.User.Roles);
        }

        [Fact]
        public async Task AssignRoleAsync_AddsRoleToUser()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var student = SeedRole(context, "Student", 3);
            var teacher = SeedRole(context, "Teacher", 2);
            var user = SeedUser(context, "roleuser@example.com", "pw", "Role User", "Active", "Student");

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.AssignRoleAsync(user.UserId, teacher.RoleId);

            Assert.True(res.Success);
            var roles = await context.UserRoles.Where(ur => ur.UserId == user.UserId).Select(ur => ur.Role.Name).ToListAsync();
            Assert.Contains("Student", roles);
            Assert.Contains("Teacher", roles);
        }

        [Fact]
        public async Task RemoveRoleAsync_RemovesRoleFromUser()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var teacher = SeedRole(context, "Teacher", 2);
            var user = SeedUser(context, "remove@example.com", "pw", "Remove User", "Active", "Teacher");

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.RemoveRoleAsync(user.UserId, teacher.RoleId);

            Assert.True(res.Success);
            var roles = await context.UserRoles.Where(ur => ur.UserId == user.UserId).ToListAsync();
            Assert.DoesNotContain(roles, ur => ur.RoleId == teacher.RoleId);
        }

        [Fact]
        public async Task GetUserRolesAsync_ReturnsRoleModels()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            SeedRole(context, "Admin", 1);
            SeedRole(context, "Student", 3);
            var user = SeedUser(context, "roles@example.com", "pw", "Roles User", "Active", "Admin", "Student");

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.GetUserRolesAsync(user.UserId);

            Assert.True(res.Success);
            Assert.NotNull(res.Data);
            var names = res.Data!.Select(r => r.Name).ToList();
            Assert.Contains("Admin", names);
            Assert.Contains("Student", names);
        }

        [Fact]
        public async Task GetUsersByRoleAsync_InvalidRole_ReturnsFailure()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.GetUsersByRoleAsync("Admin");

            Assert.False(res.Success);
            Assert.Equal("Only 'Student' or 'Teacher' roles are allowed", res.Message);
            Assert.Null(res.Data);
        }

        [Fact]
        public async Task GetUsersByRoleAsync_Teacher_ReturnsUserDtos()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            SeedRole(context, "Teacher", 2);
            SeedUser(context, "t1@example.com", "pw", "Teacher One", "Active", "Teacher");
            SeedUser(context, "t2@example.com", "pw", "Teacher Two", "Active", "Teacher");

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.GetUsersByRoleAsync("Teacher");

            Assert.True(res.Success);
            Assert.NotNull(res.Data);
            Assert.Equal(2, res.Data!.Count());
            Assert.All(res.Data!, u => Assert.Contains("Teacher", u.Roles));
        }

        [Fact]
        public async Task GetAllTeachersWithProfileAsync_ReturnsTeacherDtosWithUser()
        {
            var context = CreateDbContext();
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            SeedRole(context, "Teacher", 2);
            var u1 = SeedUser(context, "tp1@example.com", "pw", "Teacher Profile 1", "Active", "Teacher");
            var u2 = SeedUser(context, "tp2@example.com", "pw", "Teacher Profile 2", "Active", "Teacher");

            var repo = new UserRepository(context, mapper, jwtOptions);
            var res = await repo.GetAllTeachersWithProfileAsync();

            Assert.True(res.Success);
            Assert.NotNull(res.Data);
            var list = res.Data!.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, t => t.User.Email == u1.Email);
            Assert.Contains(list, t => t.User.Email == u2.Email);
        }
    }
}