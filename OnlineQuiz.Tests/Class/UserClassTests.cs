using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineQuiz.Class;
using OnlineQuiz.Configuration;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.Mappings;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.Utilities;
using Xunit;

namespace OnlineQuiz.Tests.Class
{
    public class UserClassTests
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
        public async Task CreateUserAsync_CreatesUser_WhenEmailNotExists()
        {
            var db = CreateDbContext(nameof(CreateUserAsync_CreatesUser_WhenEmailNotExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var sut = new UserClass(db, mapper, jwtOptions);

            var dto = new CreateUserDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "P@ssw0rd"
            };

            var response = await sut.CreateUserAsync(dto);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(dto.Email, response.Data!.Email);

            var saved = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            Assert.NotNull(saved);
            Assert.True(PasswordHelper.VerifyPassword(dto.Password, saved!.PasswordHash));
        }

        [Fact]
        public async Task CreateUserAsync_Fails_WhenEmailExists()
        {
            var db = CreateDbContext(nameof(CreateUserAsync_Fails_WhenEmailExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var existing = CreateUser("dup@example.com", "Secret");
            db.Users.Add(existing);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.CreateUserAsync(new CreateUserDto
            {
                FullName = "Dup User",
                Email = "dup@example.com",
                Password = "Secret"
            });

            Assert.False(response.Success);
            Assert.Equal("User with this email already exists", response.Message);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
        {
            var db = CreateDbContext(nameof(GetUserByIdAsync_ReturnsUser_WhenExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("byid@example.com", "Pass", "By Id");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.GetUserByIdAsync(user.UserId);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(user.Email, response.Data!.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_Fails_WhenNotExists()
        {
            var db = CreateDbContext(nameof(GetUserByIdAsync_Fails_WhenNotExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.GetUserByIdAsync(99999);
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenExists()
        {
            var db = CreateDbContext(nameof(GetUserByEmailAsync_ReturnsUser_WhenExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("lookup@example.com", "Pass", "Lookup User");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.GetUserByEmailAsync("lookup@example.com");

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(user.Email, response.Data!.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_Fails_WhenNotExists()
        {
            var db = CreateDbContext(nameof(GetUserByEmailAsync_Fails_WhenNotExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.GetUserByEmailAsync("missing@example.com");
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesFields_WhenUserExists()
        {
            var db = CreateDbContext(nameof(UpdateUserAsync_UpdatesFields_WhenUserExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("update@example.com", "Pass", "Original Name");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.UpdateUserAsync(user.UserId, new UpdateUserDto
            {
                FullName = "Updated Name"
            });

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("Updated Name", response.Data!.FullName);
        }

        [Fact]
        public async Task UpdateUserAsync_Fails_WhenUserMissing()
        {
            var db = CreateDbContext(nameof(UpdateUserAsync_Fails_WhenUserMissing));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.UpdateUserAsync(1234, new UpdateUserDto { FullName = "X" });
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }

        [Fact]
        public async Task DeleteUserAsync_RemovesUser_WhenExists()
        {
            var db = CreateDbContext(nameof(DeleteUserAsync_RemovesUser_WhenExists));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("del@example.com", "Pass");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.DeleteUserAsync(user.UserId);

            Assert.True(response.Success);
            Assert.Null(await db.Users.FindAsync(user.UserId));
        }

        [Fact]
        public async Task DeleteUserAsync_Fails_WhenMissing()
        {
            var db = CreateDbContext(nameof(DeleteUserAsync_Fails_WhenMissing));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.DeleteUserAsync(98765);
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsList()
        {
            var db = CreateDbContext(nameof(GetAllUsersAsync_ReturnsList));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            db.Users.AddRange(
                CreateUser("a@example.com", "P1"),
                CreateUser("b@example.com", "P2")
            );
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.GetAllUsersAsync();

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data!.Count());
        }

        [Fact]
        public async Task LoginAsync_ReturnsToken_OnValidCredentials()
        {
            var db = CreateDbContext(nameof(LoginAsync_ReturnsToken_OnValidCredentials));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("login@example.com", "P@ssw0rd", "Login User");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.LoginAsync(new LoginDto
            {
                Email = "login@example.com",
                Password = "P@ssw0rd"
            });

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.False(string.IsNullOrWhiteSpace(response.Data!.AccessToken));
            Assert.NotNull(response.Data.User);
            Assert.Equal(user.Email, response.Data.User!.Email);
        }

        [Fact]
        public async Task LoginAsync_Fails_OnInvalidCredentials()
        {
            var db = CreateDbContext(nameof(LoginAsync_Fails_OnInvalidCredentials));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("login2@example.com", "Correct");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.LoginAsync(new LoginDto
            {
                Email = "login2@example.com",
                Password = "Wrong"
            });

            Assert.False(response.Success);
            Assert.Equal("Invalid email or password", response.Message);
        }

        [Fact]
        public async Task AssignRoleAsync_AddsRole()
        {
            var db = CreateDbContext(nameof(AssignRoleAsync_AddsRole));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("role@example.com", "Pass");
            db.Users.Add(user);
            db.Roles.Add(new RoleModel { RoleId = 1, Name = "Student" });
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.AssignRoleAsync(user.UserId, 1);
            Assert.True(response.Success);

            var hasRole = await db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId && ur.RoleId == 1);
            Assert.True(hasRole);
        }

        [Fact]
        public async Task RemoveRoleAsync_RemovesExistingRole()
        {
            var db = CreateDbContext(nameof(RemoveRoleAsync_RemovesExistingRole));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("removerole@example.com", "Pass");
            db.Users.Add(user);
            db.Roles.Add(new RoleModel { RoleId = 2, Name = "Teacher" });
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRoleModel { UserId = user.UserId, RoleId = 2 });
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.RemoveRoleAsync(user.UserId, 2);
            Assert.True(response.Success);

            var stillExists = await db.UserRoles.AnyAsync(ur => ur.UserId == user.UserId && ur.RoleId == 2);
            Assert.False(stillExists);
        }

        [Fact]
        public async Task RemoveRoleAsync_Fails_WhenRoleMissing()
        {
            var db = CreateDbContext(nameof(RemoveRoleAsync_Fails_WhenRoleMissing));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("removerolemissing@example.com", "Pass");
            db.Users.Add(user);
            db.Roles.Add(new RoleModel { RoleId = 3, Name = "Student" });
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.RemoveRoleAsync(user.UserId, 3);
            Assert.False(response.Success);
            Assert.Equal("User role not found", response.Message);
        }

        [Fact]
        public async Task GetUserRolesAsync_ReturnsRoles()
        {
            var db = CreateDbContext(nameof(GetUserRolesAsync_ReturnsRoles));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var user = CreateUser("getroles@example.com", "Pass");
            db.Users.Add(user);
            db.Roles.Add(new RoleModel { RoleId = 4, Name = "Student" });
            db.Roles.Add(new RoleModel { RoleId = 5, Name = "Teacher" });
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRoleModel { UserId = user.UserId, RoleId = 4 });
            db.UserRoles.Add(new UserRoleModel { UserId = user.UserId, RoleId = 5 });
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.GetUserRolesAsync(user.UserId);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data!.Count());
            Assert.Contains(response.Data!, r => r.Name == "Student");
            Assert.Contains(response.Data!, r => r.Name == "Teacher");
        }

        [Fact]
        public async Task GetUsersByRoleAsync_ReturnsOnlyAllowedRoles()
        {
            var db = CreateDbContext(nameof(GetUsersByRoleAsync_ReturnsOnlyAllowedRoles));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();
            var studentRole = new RoleModel { RoleId = 6, Name = "Student" };
            var teacherRole = new RoleModel { RoleId = 7, Name = "Teacher" };
            db.Roles.AddRange(studentRole, teacherRole);
            var student = CreateUser("stu@example.com", "Pass", "Student User");
            var teacher = CreateUser("teach@example.com", "Pass", "Teacher User");
            db.Users.AddRange(student, teacher);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRoleModel { UserId = student.UserId, RoleId = studentRole.RoleId });
            db.UserRoles.Add(new UserRoleModel { UserId = teacher.UserId, RoleId = teacherRole.RoleId });
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var studentsResp = await sut.GetUsersByRoleAsync("Student");
            var teachersResp = await sut.GetUsersByRoleAsync("Teacher");
            var invalidResp = await sut.GetUsersByRoleAsync("Admin");

            Assert.True(studentsResp.Success);
            Assert.True(teachersResp.Success);
            Assert.False(invalidResp.Success);
            Assert.Equal("Only 'Student' or 'Teacher' roles are allowed", invalidResp.Message);
        }

        [Fact]
        public async Task GetAllTeachersWithProfileAsync_ReturnsTeachers()
        {
            var db = CreateDbContext(nameof(GetAllTeachersWithProfileAsync_ReturnsTeachers));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var roleTeacher = new RoleModel { RoleId = 8, Name = "Teacher" };
            db.Roles.Add(roleTeacher);
            var user = CreateUser("teacher@example.com", "Pass", "Teacher User");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRoleModel { UserId = user.UserId, RoleId = roleTeacher.RoleId });
            var teacher = new TeacherModel { UserId = user.UserId, User = user };
            db.Teachers.Add(teacher);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.GetAllTeachersWithProfileAsync();
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Single(response.Data!);
        }

        [Fact]
        public async Task GetAllStudentsWithProfileAsync_ReturnsStudents()
        {
            var db = CreateDbContext(nameof(GetAllStudentsWithProfileAsync_ReturnsStudents));
            var mapper = CreateMapper();
            var jwtOptions = CreateJwtOptions();

            var roleStudent = new RoleModel { RoleId = 9, Name = "Student" };
            db.Roles.Add(roleStudent);
            var user = CreateUser("student@example.com", "Pass", "Student User");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRoleModel { UserId = user.UserId, RoleId = roleStudent.RoleId });
            var student = new StudentModel { UserId = user.UserId, User = user };
            db.Students.Add(student);
            await db.SaveChangesAsync();

            var sut = new UserClass(db, mapper, jwtOptions);
            var response = await sut.GetAllStudentsWithProfileAsync();
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Single(response.Data!);
        }
    }
}