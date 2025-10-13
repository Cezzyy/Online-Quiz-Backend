using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineQuiz.Controllers;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;
using Xunit;

namespace OnlineQuiz.Tests.Controllers
{
    public class CourseControllerTests
    {
        private static CourseController CreateController(Mock<ICourseService> mockService)
        {
            return new CourseController(mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithServiceResponse()
        {
            // Arrange
            var mockService = new Mock<ICourseService>();
            var expectedCourses = new List<CourseDTO.CourseDto>
            {
                new CourseDTO.CourseDto { CourseId = 1, Code = "CS101", Name = "Intro CS", InstructorUserId = 10, InstructorName = "Prof. A" },
                new CourseDTO.CourseDto { CourseId = 2, Code = "CS102", Name = "Data Structures", InstructorUserId = 11, InstructorName = "Prof. B" }
            };
            var expectedResponse = new ServiceResponse<IEnumerable<CourseDTO.CourseDto>>(expectedCourses);
            mockService.Setup(s => s.GetAllCoursesAsync()).ReturnsAsync(expectedResponse);

            var controller = CreateController(mockService);

            // Act
            var result = await controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>>(ok.Value);
            Assert.True(payload.Success);
            Assert.NotNull(payload.Data);
            Assert.Equal(2, payload.Data!.Count());
            Assert.Contains(payload.Data!, c => c.Code == "CS101" && c.Name == "Intro CS");
            Assert.Contains(payload.Data!, c => c.Code == "CS102" && c.Name == "Data Structures");
        }

        [Fact]
        public async Task GetById_ReturnsOk_WithRequestedCourse()
        {
            // Arrange
            var mockService = new Mock<ICourseService>();
            var expectedCourse = new CourseDTO.CourseDto { CourseId = 5, Code = "MATH201", Name = "Linear Algebra", InstructorUserId = 15, InstructorName = "Prof. L" };
            var expectedResponse = new ServiceResponse<CourseDTO.CourseDto>(expectedCourse);
            mockService.Setup(s => s.GetCourseByIdAsync(5)).ReturnsAsync(expectedResponse);

            var controller = CreateController(mockService);

            // Act
            var result = await controller.GetById(5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ServiceResponse<CourseDTO.CourseDto>>(ok.Value);
            Assert.True(payload.Success);
            Assert.NotNull(payload.Data);
            Assert.Equal(5, payload.Data!.CourseId);
            Assert.Equal("MATH201", payload.Data!.Code);
            Assert.Equal("Linear Algebra", payload.Data!.Name);
            Assert.Equal(15, payload.Data!.InstructorUserId);
        }

        [Fact]
        public async Task Create_ReturnsOk_WithCreatedCourse()
        {
            // Arrange
            var mockService = new Mock<ICourseService>();
            var createDto = new CourseDTO.CreateCourseDto { Code = "PHY101", Name = "Physics I", InstructorUserId = 20 };
            var createdCourse = new CourseDTO.CourseDto { CourseId = 100, Code = createDto.Code, Name = createDto.Name, InstructorUserId = createDto.InstructorUserId, InstructorName = "Prof. P" };
            var expectedResponse = new ServiceResponse<CourseDTO.CourseDto>(createdCourse);
            mockService.Setup(s => s.CreateCourseAsync(It.IsAny<CourseDTO.CreateCourseDto>()))
                       .ReturnsAsync(expectedResponse);

            var controller = CreateController(mockService);

            // Act
            var result = await controller.Create(createDto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ServiceResponse<CourseDTO.CourseDto>>(ok.Value);
            Assert.True(payload.Success);
            Assert.NotNull(payload.Data);
            Assert.Equal(100, payload.Data!.CourseId);
            Assert.Equal("PHY101", payload.Data!.Code);
            Assert.Equal("Physics I", payload.Data!.Name);
        }

        [Fact]
        public async Task Update_ReturnsOk_WithUpdatedCourse()
        {
            // Arrange
            var mockService = new Mock<ICourseService>();
            var updateDto = new CourseDTO.UpdateCourseDto { Name = "Advanced Physics" };
            var updatedCourse = new CourseDTO.CourseDto { CourseId = 200, Code = "PHY201", Name = "Advanced Physics", InstructorUserId = 21, InstructorName = "Prof. Q" };
            var expectedResponse = new ServiceResponse<CourseDTO.CourseDto>(updatedCourse);
            mockService.Setup(s => s.UpdateCourseAsync(200, It.IsAny<CourseDTO.UpdateCourseDto>()))
                       .ReturnsAsync(expectedResponse);

            var controller = CreateController(mockService);

            // Act
            var result = await controller.Update(200, updateDto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ServiceResponse<CourseDTO.CourseDto>>(ok.Value);
            Assert.True(payload.Success);
            Assert.NotNull(payload.Data);
            Assert.Equal(200, payload.Data!.CourseId);
            Assert.Equal("Advanced Physics", payload.Data!.Name);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WithSuccessTrue()
        {
            // Arrange
            var mockService = new Mock<ICourseService>();
            var expectedResponse = new ServiceResponse<bool>(true);
            mockService.Setup(s => s.DeleteCourseAsync(300)).ReturnsAsync(expectedResponse);

            var controller = CreateController(mockService);

            // Act
            var result = await controller.Delete(300);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ServiceResponse<bool>>(ok.Value);
            Assert.True(payload.Success);
            Assert.True(payload.Data is true);
        }

        // Attribute checks to mirror controller annotations
        [Fact]
        public void Controller_HasAuthorizeAndApiControllerAttributes()
        {
            var type = typeof(CourseController);
            var hasAuthorize = type.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Any();
            var hasApiController = type.GetCustomAttributes(typeof(ApiControllerAttribute), inherit: true).Any();
            Assert.True(hasAuthorize);
            Assert.True(hasApiController);
        }

        [Fact]
        public void Controller_HasRouteAttributeWithExpectedTemplate()
        {
            var type = typeof(CourseController);
            var routeAttr = type.GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                                .Cast<RouteAttribute>()
                                .FirstOrDefault();
            Assert.NotNull(routeAttr);
            Assert.Equal("api/[controller]", routeAttr!.Template);
        }

        [Fact]
        public void Endpoints_HaveExpectedHttpMethodAttributes()
        {
            var type = typeof(CourseController);

            var getAll = type.GetMethod("GetAll");
            var getAllAttr = Assert.Single(getAll!.GetCustomAttributes(typeof(HttpGetAttribute), inherit: true));
            Assert.IsType<HttpGetAttribute>(getAllAttr);

            var getById = type.GetMethod("GetById");
            var getByIdAttr = Assert.Single(getById!.GetCustomAttributes(typeof(HttpGetAttribute), inherit: true));
            Assert.IsType<HttpGetAttribute>(getByIdAttr);
            var getByIdTemplate = ((HttpGetAttribute)getByIdAttr).Template;
            Assert.Equal("{id:long}", getByIdTemplate);

            var create = type.GetMethod("Create");
            var createAttr = Assert.Single(create!.GetCustomAttributes(typeof(HttpPostAttribute), inherit: true));
            Assert.IsType<HttpPostAttribute>(createAttr);

            var update = type.GetMethod("Update");
            var updateAttr = Assert.Single(update!.GetCustomAttributes(typeof(HttpPutAttribute), inherit: true));
            Assert.IsType<HttpPutAttribute>(updateAttr);
            var updateTemplate = ((HttpPutAttribute)updateAttr).Template;
            Assert.Equal("{id:long}", updateTemplate);

            var delete = type.GetMethod("Delete");
            var deleteAttr = Assert.Single(delete!.GetCustomAttributes(typeof(HttpDeleteAttribute), inherit: true));
            Assert.IsType<HttpDeleteAttribute>(deleteAttr);
            var deleteTemplate = ((HttpDeleteAttribute)deleteAttr).Template;
            Assert.Equal("{id:long}", deleteTemplate);
        }
    }
}