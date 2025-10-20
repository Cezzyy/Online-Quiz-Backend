using System;
using System.Collections.Generic;
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
    public class QuizRepositoryTests
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

        private static async Task<CourseModel> SeedCourseAsync(OnlineQuizDbContext context, string code = "CS101", string name = "Intro to CS", string instructorName = "Instructor One")
        {
            var instructorUserId = await SeedTeacherWithUserAsync(context, email: $"{Guid.NewGuid()}@example.com", name: instructorName);
            var teacher = await context.Teachers.FirstAsync(t => t.UserId == instructorUserId);

            var creator = new UserModel
            {
                Email = $"creator_{Guid.NewGuid()}@example.com",
                PasswordHash = "hash",
                FullName = "Course Creator",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(creator);
            await context.SaveChangesAsync();

            var course = new CourseModel
            {
                Code = code,
                Name = name,
                InstructorUserId = instructorUserId,
                Instructor = teacher,
                CreatedBy = creator.UserId,
                Creator = creator
            };
            context.Courses.Add(course);
            await context.SaveChangesAsync();
            return course;
        }

        [Fact]
        public async Task CreateQuizAsync_Creates_And_Maps_Dto()
        {
            var db = CreateDbContext(nameof(CreateQuizAsync_Creates_And_Maps_Dto));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var course = await SeedCourseAsync(db, code: "CS1", name: "Course One", instructorName: "Prof. Alpha");

            var dto = new QuizDTO.CreateQuizDto
            {
                CourseId = course.CourseId,
                Title = "Quiz A",
                DueAt = DateTime.UtcNow.AddDays(7),
                TimeLimitMinutes = 30,
                IsPublished = false
            };

            var response = await repo.CreateQuizAsync(dto, course.InstructorUserId);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(course.CourseId, response.Data!.CourseId);
            Assert.Equal("Quiz A", response.Data!.Title);
            Assert.Equal("Course One", response.Data!.CourseName);
            Assert.Equal(0, response.Data!.QuestionsCount);
            Assert.Equal(0, response.Data!.AttemptsCount);
        }

        [Fact]
        public async Task CreateQuizAsync_Returns_NotFound_When_Course_Missing()
        {
            var db = CreateDbContext(nameof(CreateQuizAsync_Returns_NotFound_When_Course_Missing));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var dto = new QuizDTO.CreateQuizDto
            {
                CourseId = 999999,
                Title = "Quiz Missing Course"
            };

            var response = await repo.CreateQuizAsync(dto, createdByUserId: 1);
            Assert.False(response.Success);
            Assert.Equal("Course not found.", response.Message);
        }

        [Fact]
        public async Task GetAllQuizzesAsync_Returns_List_With_CourseName()
        {
            var db = CreateDbContext(nameof(GetAllQuizzesAsync_Returns_List_With_CourseName));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var course = await SeedCourseAsync(db, code: "MATH1", name: "Algebra", instructorName: "Prof. Smith");
            await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = course.CourseId, Title = "Quiz 1" }, course.InstructorUserId);
            await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = course.CourseId, Title = "Quiz 2" }, course.InstructorUserId);

            var response = await repo.GetAllQuizzesAsync();
            Assert.True(response.Success);
            var quizzes = Assert.IsAssignableFrom<IEnumerable<QuizDTO.QuizDto>>(response.Data!);
            Assert.Equal(2, quizzes.Count());
            Assert.All(quizzes, q => Assert.Equal("Algebra", q.CourseName));
        }

        [Fact]
        public async Task GetQuizByIdAsync_Returns_Quiz_When_Found()
        {
            var db = CreateDbContext(nameof(GetQuizByIdAsync_Returns_Quiz_When_Found));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var course = await SeedCourseAsync(db, code: "PHY1", name: "Physics", instructorName: "Dr. Jane");
            var created = await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = course.CourseId, Title = "Midterm" }, course.InstructorUserId);

            var response = await repo.GetQuizByIdAsync(created.Data!.QuizId);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal("Midterm", response.Data!.Title);
            Assert.Equal("Physics", response.Data!.CourseName);
        }

        [Fact]
        public async Task GetQuizByIdAsync_Returns_NotFound_When_Missing()
        {
            var db = CreateDbContext(nameof(GetQuizByIdAsync_Returns_NotFound_When_Missing));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var response = await repo.GetQuizByIdAsync(9999);
            Assert.False(response.Success);
            Assert.Equal("Quiz not found.", response.Message);
        }

        [Fact]
        public async Task UpdateQuizAsync_Updates_Fields_When_Found()
        {
            var db = CreateDbContext(nameof(UpdateQuizAsync_Updates_Fields_When_Found));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var course = await SeedCourseAsync(db, code: "HIST1", name: "History", instructorName: "Prof. Beta");
            var created = await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = course.CourseId, Title = "Quiz Old" }, course.InstructorUserId);

            var update = new QuizDTO.UpdateQuizDto { Title = "Quiz New", IsPublished = true, TimeLimitMinutes = 45 };
            var response = await repo.UpdateQuizAsync(created.Data!.QuizId, update);

            Assert.True(response.Success);
            Assert.NotNull(response.Data.UpdatedQuiz);
            Assert.Equal("Quiz New", response.Data!.UpdatedQuiz.Title);
            Assert.True(response.Data!.UpdatedQuiz.IsPublished);
            Assert.Equal(45, response.Data!.UpdatedQuiz.TimeLimitMinutes);
            Assert.NotNull(response.Data!.OldValues);
            Assert.Equal("History", response.Data!.UpdatedQuiz.CourseName);
        }

        [Fact]
        public async Task UpdateQuizAsync_Returns_NotFound_When_Missing()
        {
            var db = CreateDbContext(nameof(UpdateQuizAsync_Returns_NotFound_When_Missing));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var update = new QuizDTO.UpdateQuizDto { Title = "Does Not Matter" };
            var response = await repo.UpdateQuizAsync(12345, update);
            Assert.False(response.Success);
            Assert.Equal("Quiz not found.", response.Message);
        }

        [Fact]
        public async Task DeleteQuizAsync_Removes_Quiz_When_Found()
        {
            var db = CreateDbContext(nameof(DeleteQuizAsync_Removes_Quiz_When_Found));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var course = await SeedCourseAsync(db, code: "BIO1", name: "Biology", instructorName: "Prof. Gamma");
            var created = await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = course.CourseId, Title = "Bio Quiz" }, course.InstructorUserId);

            var response = await repo.DeleteQuizAsync(created.Data!.QuizId);
            Assert.True(response.Success);
            Assert.True(response.Data!.Deleted);
            Assert.NotNull(response.Data!.QuizInfo);
            Assert.Equal(0, await db.Quizzes.CountAsync());
        }

        [Fact]
        public async Task DeleteQuizAsync_Returns_NotFound_When_Missing()
        {
            var db = CreateDbContext(nameof(DeleteQuizAsync_Returns_NotFound_When_Missing));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var response = await repo.DeleteQuizAsync(54321);
            Assert.False(response.Success);
            Assert.Equal("Quiz not found.", response.Message);
        }

        [Fact]
        public async Task GetQuizzesByCourseAsync_Returns_Filtered_List()
        {
            var db = CreateDbContext(nameof(GetQuizzesByCourseAsync_Returns_Filtered_List));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            var courseA = await SeedCourseAsync(db, code: "ENG1", name: "English", instructorName: "Prof. Delta");
            var courseB = await SeedCourseAsync(db, code: "CHEM1", name: "Chemistry", instructorName: "Prof. Epsilon");

            await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = courseA.CourseId, Title = "English Quiz" }, courseA.InstructorUserId);
            await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = courseB.CourseId, Title = "Chemistry Quiz" }, courseB.InstructorUserId);

            var response = await repo.GetQuizzesByCourseAsync(courseA.CourseId);
            Assert.True(response.Success);
            var quizzes = Assert.IsAssignableFrom<IEnumerable<QuizDTO.QuizDto>>(response.Data!);
            Assert.Single(quizzes);
            Assert.All(quizzes, q => Assert.Equal(courseA.CourseId, q.CourseId));
            Assert.All(quizzes, q => Assert.Equal("English", q.CourseName));
        }

        [Fact]
        public async Task GetQuizzesByInstructorAsync_Returns_Filtered_List()
        {
            var db = CreateDbContext(nameof(GetQuizzesByInstructorAsync_Returns_Filtered_List));
            var mapper = CreateMapper();
            var repo = new QuizRepository(db, mapper);

            // Instructor A with two courses
            var instructorAId = await SeedTeacherWithUserAsync(db, email: $"a_{Guid.NewGuid()}@example.com", name: "Instructor A");
            var teacherA = await db.Teachers.FirstAsync(t => t.UserId == instructorAId);
            var creatorA = new UserModel { Email = $"creatorA_{Guid.NewGuid()}@example.com", PasswordHash = "hash", FullName = "Creator A", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Users.Add(creatorA);
            await db.SaveChangesAsync();
            var courseA1 = new CourseModel { Code = "A1", Name = "Course A1", InstructorUserId = instructorAId, Instructor = teacherA, CreatedBy = creatorA.UserId, Creator = creatorA };
            var courseA2 = new CourseModel { Code = "A2", Name = "Course A2", InstructorUserId = instructorAId, Instructor = teacherA, CreatedBy = creatorA.UserId, Creator = creatorA };
            db.Courses.AddRange(courseA1, courseA2);
            await db.SaveChangesAsync();

            // Instructor B with one course
            var instructorBId = await SeedTeacherWithUserAsync(db, email: $"b_{Guid.NewGuid()}@example.com", name: "Instructor B");
            var teacherB = await db.Teachers.FirstAsync(t => t.UserId == instructorBId);
            var creatorB = new UserModel { Email = $"creatorB_{Guid.NewGuid()}@example.com", PasswordHash = "hash", FullName = "Creator B", Status = "Active", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Users.Add(creatorB);
            await db.SaveChangesAsync();
            var courseB = new CourseModel { Code = "B1", Name = "Course B1", InstructorUserId = instructorBId, Instructor = teacherB, CreatedBy = creatorB.UserId, Creator = creatorB };
            db.Courses.Add(courseB);
            await db.SaveChangesAsync();

            // Create quizzes across courses
            await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = courseA1.CourseId, Title = "A1 Quiz" }, instructorAId);
            await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = courseA2.CourseId, Title = "A2 Quiz" }, instructorAId);
            await repo.CreateQuizAsync(new QuizDTO.CreateQuizDto { CourseId = courseB.CourseId, Title = "B1 Quiz" }, instructorBId);

            var response = await repo.GetQuizzesByInstructorAsync(instructorAId);
            Assert.True(response.Success);
            var quizzes = Assert.IsAssignableFrom<IEnumerable<QuizDTO.QuizDto>>(response.Data!);
            Assert.Equal(2, quizzes.Count());
            Assert.Contains(quizzes, q => q.Title == "A1 Quiz");
            Assert.Contains(quizzes, q => q.Title == "A2 Quiz");
            Assert.DoesNotContain(quizzes, q => q.Title == "B1 Quiz");
        }
    }
}