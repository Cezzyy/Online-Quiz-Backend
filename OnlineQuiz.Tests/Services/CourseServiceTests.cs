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
    public class CourseServiceTests
    {
        private static CourseService CreateService(Mock<ICourseRepository> mockRepo)
        {
            return new CourseService(mockRepo.Object);
        }

        [Fact]
        public async Task GetAllCoursesAsync_ForwardsResponseFromRepository()
        {
            // Arrange
            var mockRepo = new Mock<ICourseRepository>();
            var courses = new List<CourseDTO.CourseDto>
            {
                new CourseDTO.CourseDto { CourseId = 1, Code = "CS101", Name = "Intro CS", InstructorUserId = 10 },
                new CourseDTO.CourseDto { CourseId = 2, Code = "CS102", Name = "Data Structures", InstructorUserId = 11 }
            };
            var expectedResponse = new ServiceResponse<IEnumerable<CourseDTO.CourseDto>>(courses);
            mockRepo.Setup(r => r.GetAllCoursesAsync()).ReturnsAsync(expectedResponse);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.GetAllCoursesAsync();

            // Assert
            Assert.Same(expectedResponse, result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data!.Count());
        }

        [Fact]
        public async Task GetCourseByIdAsync_UsesIdAndForwardsResponse()
        {
            // Arrange
            var mockRepo = new Mock<ICourseRepository>();
            var course = new CourseDTO.CourseDto { CourseId = 5, Code = "MATH201", Name = "Linear Algebra", InstructorUserId = 15 };
            var expectedResponse = new ServiceResponse<CourseDTO.CourseDto>(course);
            mockRepo.Setup(r => r.GetCourseByIdAsync(5)).ReturnsAsync(expectedResponse);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.GetCourseByIdAsync(5);

            // Assert
            Assert.Same(expectedResponse, result);
            mockRepo.Verify(r => r.GetCourseByIdAsync(5), Times.Once);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(5, result.Data!.CourseId);
        }

        [Fact]
        public async Task CreateCourseAsync_ForwardsRequestAndResponse()
        {
            // Arrange
            var mockRepo = new Mock<ICourseRepository>();
            var createDto = new CourseDTO.CreateCourseDto { Code = "PHY101", Name = "Physics I", InstructorUserId = 20 };
            var created = new CourseDTO.CourseDto { CourseId = 100, Code = createDto.Code, Name = createDto.Name, InstructorUserId = createDto.InstructorUserId };
            var expectedResponse = new ServiceResponse<CourseDTO.CourseDto>(created);
            mockRepo.Setup(r => r.CreateCourseAsync(It.IsAny<CourseDTO.CreateCourseDto>(), It.IsAny<long>()))
                    .ReturnsAsync(expectedResponse);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.CreateCourseAsync(createDto, 1L);

            // Assert
            Assert.Same(expectedResponse, result);
            mockRepo.Verify(r => r.CreateCourseAsync(
                It.Is<CourseDTO.CreateCourseDto>(d => d.Code == "PHY101" && d.Name == "Physics I" && d.InstructorUserId == 20),
                It.Is<long>(id => id == 1L)
            ), Times.Once);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(100, result.Data!.CourseId);
        }

        [Fact]
        public async Task UpdateCourseAsync_ForwardsRequestAndResponse()
        {
            // Arrange
            var mockRepo = new Mock<ICourseRepository>();
            var updateDto = new CourseDTO.UpdateCourseDto { Name = "Advanced Physics" };
            var updated = new CourseDTO.CourseDto { CourseId = 200, Code = "PHY201", Name = "Advanced Physics", InstructorUserId = 21 };
            var expectedResponse = new ServiceResponse<(CourseDTO.CourseDto UpdatedCourse, object OldValues)>((updated, new { Name = "Old Name" }));
            mockRepo.Setup(r => r.UpdateCourseAsync(200, It.IsAny<CourseDTO.UpdateCourseDto>()))
                    .ReturnsAsync(expectedResponse);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.UpdateCourseAsync(200, updateDto);

            // Assert
            Assert.Same(expectedResponse, result);
            mockRepo.Verify(r => r.UpdateCourseAsync(200, It.Is<CourseDTO.UpdateCourseDto>(d => d.Name == "Advanced Physics")), Times.Once);
            Assert.True(result.Success);
            Assert.NotNull(result.Data.UpdatedCourse);
            Assert.Equal(200, result.Data!.UpdatedCourse.CourseId);
        }

        [Fact]
        public async Task DeleteCourseAsync_ForwardsResponse()
        {
            // Arrange
            var mockRepo = new Mock<ICourseRepository>();
            var expectedResponse = new ServiceResponse<(bool Deleted, object CourseInfo)>((true, new { CourseId = 300, Code = "BIO101", Name = "Biology" }));
            mockRepo.Setup(r => r.DeleteCourseAsync(300)).ReturnsAsync(expectedResponse);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.DeleteCourseAsync(300);

            // Assert
            Assert.Same(expectedResponse, result);
            mockRepo.Verify(r => r.DeleteCourseAsync(300), Times.Once);
            Assert.True(result.Success);
            Assert.True(result.Data!.Deleted);
        }

        [Fact]
        public async Task GetCourseByIdAsync_WhenRepositoryFails_ReturnsSameFailure()
        {
            // Arrange
            var mockRepo = new Mock<ICourseRepository>();
            var failure = ServiceResponse<CourseDTO.CourseDto>.Fail("Course not found.");
            mockRepo.Setup(r => r.GetCourseByIdAsync(999)).ReturnsAsync(failure);

            var service = CreateService(mockRepo);

            // Act
            var result = await service.GetCourseByIdAsync(999);

            // Assert
            Assert.Same(failure, result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("Course not found.", result.Message);
        }
    }
}