using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models.Response;
using OnlineQuiz.Services;
using Xunit;

namespace OnlineQuiz.Tests.Services
{
    public class QuizServiceTests
    {
        private static QuizService CreateService(Mock<IQuizRepository> mockRepo) => new QuizService(mockRepo.Object);

        [Fact]
        public async Task GetAllQuizzesAsync_Returns_List()
        {
            var mockRepo = new Mock<IQuizRepository>();
            var quizzes = new List<QuizDTO.QuizDto>
            {
                new QuizDTO.QuizDto { QuizId = 1, CourseId = 10, Title = "Quiz A" },
                new QuizDTO.QuizDto { QuizId = 2, CourseId = 11, Title = "Quiz B" }
            };
            var expected = new ServiceResponse<IEnumerable<QuizDTO.QuizDto>>(quizzes);
            mockRepo.Setup(r => r.GetAllQuizzesAsync()).ReturnsAsync(expected);

            var service = CreateService(mockRepo);
            var result = await service.GetAllQuizzesAsync();

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data!.Count());
            Assert.Contains(result.Data!, q => q.Title == "Quiz A");
            Assert.Contains(result.Data!, q => q.Title == "Quiz B");
            mockRepo.Verify(r => r.GetAllQuizzesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetQuizByIdAsync_Returns_Quiz_When_Found()
        {
            var mockRepo = new Mock<IQuizRepository>();
            var quiz = new QuizDTO.QuizDto { QuizId = 5, CourseId = 20, Title = "Midterm" };
            var expected = new ServiceResponse<QuizDTO.QuizDto>(quiz);
            mockRepo.Setup(r => r.GetQuizByIdAsync(5)).ReturnsAsync(expected);

            var service = CreateService(mockRepo);
            var result = await service.GetQuizByIdAsync(5);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(5, result.Data!.QuizId);
            Assert.Equal("Midterm", result.Data!.Title);
            mockRepo.Verify(r => r.GetQuizByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task CreateQuizAsync_Returns_CreatedQuiz_And_PassesCreatedByUserId()
        {
            var mockRepo = new Mock<IQuizRepository>();
            var dto = new QuizDTO.CreateQuizDto { CourseId = 10, Title = "Final" };
            var created = new QuizDTO.QuizDto { QuizId = 100, CourseId = 10, Title = "Final" };
            var expected = new ServiceResponse<QuizDTO.QuizDto>(created);
            long createdByUserId = 1;
            mockRepo.Setup(r => r.CreateQuizAsync(dto, createdByUserId)).ReturnsAsync(expected);

            var service = CreateService(mockRepo);
            var result = await service.CreateQuizAsync(dto, createdByUserId);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(100, result.Data!.QuizId);
            Assert.Equal("Final", result.Data!.Title);
            mockRepo.Verify(r => r.CreateQuizAsync(It.IsAny<QuizDTO.CreateQuizDto>(), createdByUserId), Times.Once);
        }

        [Fact]
        public async Task UpdateQuizAsync_Returns_UpdatedQuiz_And_Verifies_Call()
        {
            var mockRepo = new Mock<IQuizRepository>();
            long id = 200;
            var dto = new QuizDTO.UpdateQuizDto { Title = "Updated" };
            var updated = new QuizDTO.QuizDto { QuizId = id, CourseId = 12, Title = "Updated" };
            var expected = new ServiceResponse<(QuizDTO.QuizDto UpdatedQuiz, object OldValues)>((updated, new { Title = "Old" }));
            mockRepo.Setup(r => r.UpdateQuizAsync(id, dto)).ReturnsAsync(expected);

            var service = CreateService(mockRepo);
            var result = await service.UpdateQuizAsync(id, dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data.UpdatedQuiz);
            Assert.Equal(id, result.Data.UpdatedQuiz.QuizId);
            Assert.Equal("Updated", result.Data.UpdatedQuiz.Title);
            mockRepo.Verify(r => r.UpdateQuizAsync(id, It.IsAny<QuizDTO.UpdateQuizDto>()), Times.Once);
        }

        [Fact]
        public async Task DeleteQuizAsync_Returns_True_And_Verifies_Call()
        {
            var mockRepo = new Mock<IQuizRepository>();
            long id = 300;
            var info = new QuizDTO.QuizDto { QuizId = id, CourseId = 10, Title = "Pop Quiz" };
            var expected = new ServiceResponse<(bool Deleted, object QuizInfo)>((true, info));
            mockRepo.Setup(r => r.DeleteQuizAsync(id)).ReturnsAsync(expected);

            var service = CreateService(mockRepo);
            var result = await service.DeleteQuizAsync(id);

            Assert.True(result.Success);
            Assert.True(result.Data.Deleted);
            Assert.IsType<QuizDTO.QuizDto>(result.Data.QuizInfo);
            mockRepo.Verify(r => r.DeleteQuizAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetQuizzesByCourseAsync_Returns_Filtered_List()
        {
            var mockRepo = new Mock<IQuizRepository>();
            long courseId = 99;
            var quizzes = new List<QuizDTO.QuizDto>
            {
                new QuizDTO.QuizDto { QuizId = 1, CourseId = courseId, Title = "Course Quiz 1" },
                new QuizDTO.QuizDto { QuizId = 2, CourseId = courseId, Title = "Course Quiz 2" }
            };
            var expected = new ServiceResponse<IEnumerable<QuizDTO.QuizDto>>(quizzes);
            mockRepo.Setup(r => r.GetQuizzesByCourseAsync(courseId)).ReturnsAsync(expected);

            var service = CreateService(mockRepo);
            var result = await service.GetQuizzesByCourseAsync(courseId);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data!.Count());
            Assert.All(result.Data!, q => Assert.Equal(courseId, q.CourseId));
            mockRepo.Verify(r => r.GetQuizzesByCourseAsync(courseId), Times.Once);
        }

        [Fact]
        public async Task GetQuizzesByInstructorAsync_Returns_Filtered_List()
        {
            var mockRepo = new Mock<IQuizRepository>();
            long instructorId = 42;
            var quizzes = new List<QuizDTO.QuizDto>
            {
                new QuizDTO.QuizDto { QuizId = 10, CourseId = 1, Title = "Instructor Quiz 1" },
                new QuizDTO.QuizDto { QuizId = 11, CourseId = 2, Title = "Instructor Quiz 2" }
            };
            var expected = new ServiceResponse<IEnumerable<QuizDTO.QuizDto>>(quizzes);
            mockRepo.Setup(r => r.GetQuizzesByInstructorAsync(instructorId)).ReturnsAsync(expected);

            var service = CreateService(mockRepo);
            var result = await service.GetQuizzesByInstructorAsync(instructorId);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data!.Count());
            Assert.Contains(result.Data!, q => q.Title == "Instructor Quiz 1");
            Assert.Contains(result.Data!, q => q.Title == "Instructor Quiz 2");
            mockRepo.Verify(r => r.GetQuizzesByInstructorAsync(instructorId), Times.Once);
        }
    }
}