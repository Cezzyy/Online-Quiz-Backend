using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.Mappings;
using OnlineQuiz.Models;
using OnlineQuiz.Repository;
using Xunit;

namespace OnlineQuiz.Tests.Repository
{
    public class CourseRepositoryTests
    {
        private static IMapper CreateMapper()
        {
            var loggerFactory = LoggerFactory.Create(builder => { builder.ClearProviders(); builder.SetMinimumLevel(LogLevel.Critical); });
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<AutoMapperProfile>(); }, loggerFactory);
            return config.CreateMapper();
        }

        private static OnlineQuizDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new OnlineQuizDbContext(options);
        }

        private static async Task<long> SeedTeacherWithUserAsync(OnlineQuizDbContext context, string email = "teacher@example.com", string name = "Instructor One")
        {
            var user = new UserModel
            {
                Email = email,
                PasswordHash = "hash",
                FullName = name,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var teacher = new TeacherModel
            {
                UserId = user.UserId,
                Department = "Science",
                User = user
            };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            return user.UserId;
        }

        [Fact]
        public async Task CreateCourseAsync_Creates_And_Maps_Dto()
        {
            var db = CreateDbContext(nameof(CreateCourseAsync_Creates_And_Maps_Dto));
            var mapper = CreateMapper();
            var repo = new CourseRepository(db, mapper);

            var instructorId = await SeedTeacherWithUserAsync(db);

            var dto = new CourseDTO.CreateCourseDto
            {
                Code = "CS101",
                Name = "Intro to CS",
                InstructorUserId = instructorId
            };

            var response = await repo.CreateCourseAsync(dto);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("CS101", response.Data!.Code);
            Assert.Equal("Intro to CS", response.Data!.Name);
            Assert.Equal(instructorId, response.Data!.InstructorUserId);

            // InstructorName mapping should resolve to User.FullName
            Assert.Equal("Instructor One", response.Data!.InstructorName);
        }

        [Fact]
        public async Task GetAllCoursesAsync_Returns_List_With_InstructorName()
        {
            var db = CreateDbContext(nameof(GetAllCoursesAsync_Returns_List_With_InstructorName));
            var mapper = CreateMapper();
            var repo = new CourseRepository(db, mapper);

            var instructorId = await SeedTeacherWithUserAsync(db, name: "Prof. Smith");

            db.Courses.Add(new CourseModel { Code = "MATH1", Name = "Algebra", InstructorUserId = instructorId });
            db.Courses.Add(new CourseModel { Code = "CS201", Name = "Data Structures", InstructorUserId = instructorId });
            await db.SaveChangesAsync();

            var response = await repo.GetAllCoursesAsync();
            Assert.True(response.Success);
            var courses = Assert.IsAssignableFrom<IEnumerable<CourseDTO.CourseDto>>(response.Data!);
            Assert.Equal(2, courses.Count());
            Assert.All(courses, c => Assert.Equal("Prof. Smith", c.InstructorName));
        }

        [Fact]
        public async Task GetCourseByIdAsync_Returns_Course_When_Found()
        {
            var db = CreateDbContext(nameof(GetCourseByIdAsync_Returns_Course_When_Found));
            var mapper = CreateMapper();
            var repo = new CourseRepository(db, mapper);

            var instructorId = await SeedTeacherWithUserAsync(db, name: "Dr. Jane");

            var course = new CourseModel { Code = "PHY101", Name = "Physics", InstructorUserId = instructorId };
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            var response = await repo.GetCourseByIdAsync(course.CourseId);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("PHY101", response.Data!.Code);
            Assert.Equal("Physics", response.Data!.Name);
            Assert.Equal("Dr. Jane", response.Data!.InstructorName);
        }

        [Fact]
        public async Task GetCourseByIdAsync_Returns_NotFound_When_Missing()
        {
            var db = CreateDbContext(nameof(GetCourseByIdAsync_Returns_NotFound_When_Missing));
            var mapper = CreateMapper();
            var repo = new CourseRepository(db, mapper);

            var response = await repo.GetCourseByIdAsync(9999);
            Assert.False(response.Success);
            Assert.Equal("Course not found.", response.Message);
        }

        [Fact]
        public async Task UpdateCourseAsync_Updates_Fields_When_Found()
        {
            var db = CreateDbContext(nameof(UpdateCourseAsync_Updates_Fields_When_Found));
            var mapper = CreateMapper();
            var repo = new CourseRepository(db, mapper);

            var instructorId = await SeedTeacherWithUserAsync(db);
            var course = new CourseModel { Code = "HIST1", Name = "History", InstructorUserId = instructorId };
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            var update = new CourseDTO.UpdateCourseDto { Name = "World History" };

            var response = await repo.UpdateCourseAsync(course.CourseId, update);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("World History", response.Data!.Name);
        }

        [Fact]
        public async Task UpdateCourseAsync_Returns_NotFound_When_Missing()
        {
            var db = CreateDbContext(nameof(UpdateCourseAsync_Returns_NotFound_When_Missing));
            var mapper = CreateMapper();
            var repo = new CourseRepository(db, mapper);

            var update = new CourseDTO.UpdateCourseDto { Name = "Does Not Matter" };
            var response = await repo.UpdateCourseAsync(12345, update);
            Assert.False(response.Success);
            Assert.Equal("Course not found.", response.Message);
        }

        [Fact]
        public async Task DeleteCourseAsync_Removes_Course_When_Found()
        {
            var db = CreateDbContext(nameof(DeleteCourseAsync_Removes_Course_When_Found));
            var mapper = CreateMapper();
            var repo = new CourseRepository(db, mapper);

            var instructorId = await SeedTeacherWithUserAsync(db);
            var course = new CourseModel { Code = "BIO101", Name = "Biology", InstructorUserId = instructorId };
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            var response = await repo.DeleteCourseAsync(course.CourseId);
            Assert.True(response.Success);
            Assert.True(response.Data);
            Assert.Equal(0, await db.Courses.CountAsync());
        }

        [Fact]
        public async Task DeleteCourseAsync_Returns_NotFound_When_Missing()
        {
            var db = CreateDbContext(nameof(DeleteCourseAsync_Returns_NotFound_When_Missing));
            var mapper = CreateMapper();
            var repo = new CourseRepository(db, mapper);

            var response = await repo.DeleteCourseAsync(54321);
            Assert.False(response.Success);
            Assert.Equal("Course not found.", response.Message);
        }
    }
}