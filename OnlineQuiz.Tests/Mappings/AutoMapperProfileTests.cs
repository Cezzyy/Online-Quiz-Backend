using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OnlineQuiz.DTOs;
using OnlineQuiz.Mappings;
using OnlineQuiz.Models;
using Xunit;

namespace OnlineQuiz.Tests.Mappings
{
    public class AutoMapperProfileTests
    {
        private readonly IMapper _mapper;

        public AutoMapperProfileTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().AddConsole());
            var config = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            }, loggerFactory);
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void UserModel_To_UserDto_Maps_Properties_And_Roles()
        {
            var user = new UserModel
            {
                UserId = 42,
                Email = "user@example.com",
                FullName = "Test User",
                Status = "Active",
                ContactNumber = "123",
                EmergencyContactNumber = "456",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
            };
            var roleTeacher = new RoleModel { RoleId = 2, Name = "Teacher" };
            var roleStudent = new RoleModel { RoleId = 3, Name = "Student" };
            user.UserRoles = new List<UserRoleModel>
            {
                new UserRoleModel { UserId = user.UserId, RoleId = roleTeacher.RoleId, Role = roleTeacher, User = user },
                new UserRoleModel { UserId = user.UserId, RoleId = roleStudent.RoleId, Role = roleStudent, User = user },
            };

            var dto = _mapper.Map<UserDto>(user);

            Assert.Equal(user.UserId, dto.UserId);
            Assert.Equal(user.Email, dto.Email);
            Assert.Equal(user.FullName, dto.FullName);
            Assert.Equal(user.Status, dto.Status);
            Assert.Equal(user.CreatedAt, dto.CreatedAt);
            Assert.Equal(user.UpdatedAt, dto.UpdatedAt);
            Assert.NotNull(dto.Roles);
            Assert.Contains("Teacher", dto.Roles);
            Assert.Contains("Student", dto.Roles);
        }

        [Fact]
        public void CreateUserDto_To_UserModel_Sets_Defaults_And_Ignores_PasswordHash()
        {
            var nowBefore = DateTime.UtcNow;
            var dto = new CreateUserDto
            {
                Email = "new@example.com",
                Password = "Password1",
                FullName = "New User",
                ContactNumber = "111",
                EmergencyContactNumber = "222",
                Roles = new List<string> { "Student" }
            };

            var model = _mapper.Map<UserModel>(dto);

            Assert.Equal(dto.Email, model.Email);
            Assert.Equal(dto.FullName, model.FullName);
            Assert.Equal("Active", model.Status);
            Assert.Equal(string.Empty, model.PasswordHash);
            Assert.Equal(0, model.UserId);

            var nowAfter = DateTime.UtcNow;
            Assert.InRange(model.CreatedAt, nowBefore.AddSeconds(-5), nowAfter.AddSeconds(5));
            Assert.InRange(model.UpdatedAt, nowBefore.AddSeconds(-5), nowAfter.AddSeconds(5));
        }

        [Fact]
        public void UpdateUserDto_To_UserModel_Only_Updates_NonNull_And_Sets_UpdatedAt()
        {
            var original = new UserModel
            {
                UserId = 5,
                Email = "keep@example.com",
                FullName = "Original Name",
                Status = "Active",
                ContactNumber = "999",
                EmergencyContactNumber = "888",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                PasswordHash = "hash"
            };

            var dto = new UpdateUserDto
            {
                FullName = "Updated Name",
                ContactNumber = null, // should not overwrite
                EmergencyContactNumber = "777",
                Status = null // should not overwrite
            };

            var beforeMap = DateTime.UtcNow;
            _mapper.Map(dto, original);
            var afterMap = DateTime.UtcNow;

            Assert.Equal("Updated Name", original.FullName);
            Assert.Equal("999", original.ContactNumber); // unchanged
            Assert.Equal("777", original.EmergencyContactNumber); // updated
            Assert.Equal("Active", original.Status); // unchanged
            Assert.InRange(original.UpdatedAt, beforeMap.AddSeconds(-5), afterMap.AddSeconds(5));
            Assert.Equal("hash", original.PasswordHash); // untouched
        }

        [Fact]
        public void TeacherModel_To_TeacherDto_Maps_Nested_User()
        {
            var user = new UserModel { UserId = 10, Email = "teacher@example.com", FullName = "Teach One", Status = "Active" };
            var teacher = new TeacherModel { UserId = user.UserId, Department = "Math", User = user };

            var dto = _mapper.Map<TeacherDto>(teacher);

            Assert.Equal(teacher.UserId, dto.UserId);
            Assert.Equal("Math", dto.Department);
            Assert.NotNull(dto.User);
            Assert.Equal(user.Email, dto.User.Email);
            Assert.Equal(user.FullName, dto.User.FullName);
        }

        [Fact]
        public void CreateTeacherDto_To_TeacherModel_Maps_Department()
        {
            var dto = new CreateTeacherDto { Department = "Science" };
            var model = _mapper.Map<TeacherModel>(dto);
            Assert.Equal("Science", model.Department);
        }

        [Fact]
        public void StudentModel_To_StudentDto_Maps_Nested_User()
        {
            var user = new UserModel { UserId = 20, Email = "student@example.com", FullName = "Stud One", Status = "Active" };
            var student = new StudentModel { UserId = user.UserId, StudentNumber = "S123", YearLevel = 2, Section = "A", Course = "CS", User = user };

            var dto = _mapper.Map<StudentDto>(student);

            Assert.Equal(student.UserId, dto.UserId);
            Assert.Equal("S123", dto.StudentNumber);
            Assert.Equal(2, dto.YearLevel);
            Assert.Equal("A", dto.Section);
            Assert.Equal("CS", dto.Course);
            Assert.NotNull(dto.User);
            Assert.Equal(user.Email, dto.User.Email);
        }

        [Fact]
        public void CreateStudentDto_To_StudentModel_Maps_All_Fields()
        {
            var dto = new CreateStudentDto { StudentNumber = "N001", YearLevel = 1, Section = "Blue", Course = "IT" };
            var model = _mapper.Map<StudentModel>(dto);
            Assert.Equal("N001", model.StudentNumber);
            Assert.Equal(1, model.YearLevel);
            Assert.Equal("Blue", model.Section);
            Assert.Equal("IT", model.Course);
        }

        [Fact]
        public void RoleModel_To_String_Maps_Name()
        {
            var role = new RoleModel { RoleId = 1, Name = "Admin" };
            var mapped = _mapper.Map<string>(role);
            Assert.Equal("Admin", mapped);
        }
    }
}