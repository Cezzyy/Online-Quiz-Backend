using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Moq;
using OnlineQuiz.Controllers;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;
using Xunit;

namespace OnlineQuiz.Tests.Controllers
{
    public class QuizControllerTests
    {
        private static QuizController CreateController(Mock<IQuizService> mockService, Mock<IActivityLogService>? mockActivityLog = null)
        {
            var controller = new QuizController(mockService.Object, (mockActivityLog ?? new Mock<IActivityLogService>()).Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "1")
                    }, "TestAuth"))
                }
            };
            return controller;
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithServiceResponse()
        {
            var mockService = new Mock<IQuizService>();
            var quizzes = new List<QuizDTO.QuizDto>
            {
                new QuizDTO.QuizDto { QuizId = 1, CourseId = 10, Title = "Quiz A" },
                new QuizDTO.QuizDto { QuizId = 2, CourseId = 11, Title = "Quiz B" }
            };
            var expected = new ServiceResponse<IEnumerable<QuizDTO.QuizDto>>(quizzes);
            mockService.Setup(s => s.GetAllQuizzesAsync()).ReturnsAsync(expected);

            var controller = CreateController(mockService);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>>(ok.Value);
            Assert.True(payload.Success);
            Assert.NotNull(payload.Data);
            Assert.Equal(2, payload.Data!.Count());
            Assert.Contains(payload.Data!, q => q.Title == "Quiz A");
            Assert.Contains(payload.Data!, q => q.Title == "Quiz B");
        }

        [Fact]
        public async Task GetById_ReturnsOk_WithRequestedQuiz()
        {
            var mockService = new Mock<IQuizService>();
            var quiz = new QuizDTO.QuizDto { QuizId = 5, CourseId = 20, Title = "Midterm" };
            var expected = new ServiceResponse<QuizDTO.QuizDto>(quiz);
            mockService.Setup(s => s.GetQuizByIdAsync(5)).ReturnsAsync(expected);

            var controller = CreateController(mockService);

            var result = await controller.GetById(5);

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ServiceResponse<QuizDTO.QuizDto>>(ok.Value);
            Assert.True(payload.Success);
            Assert.NotNull(payload.Data);
            Assert.Equal(5, payload.Data!.QuizId);
            Assert.Equal("Midterm", payload.Data!.Title);
        }

        [Fact]
        public async Task Create_ReturnsOk_WithCreatedQuiz()
        {
            var mockService = new Mock<IQuizService>();
            var createDto = new QuizDTO.CreateQuizDto { CourseId = 10, Title = "Final" };
            var created = new QuizDTO.QuizDto { QuizId = 100, CourseId = 10, Title = "Final" };
            var expected = new ServiceResponse<QuizDTO.QuizDto>(created);
            mockService.Setup(s => s.CreateQuizAsync(It.IsAny<QuizDTO.CreateQuizDto>(), It.IsAny<long>()))
                       .ReturnsAsync(expected);

            var controller = CreateController(mockService);

            var result = await controller.Create(createDto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<ServiceResponse<QuizDTO.QuizDto>>(ok.Value);
            Assert.True(payload.Success);
            Assert.NotNull(payload.Data);
            Assert.Equal(100, payload.Data!.QuizId);
            Assert.Equal("Final", payload.Data!.Title);
        }

        [Fact]
        public async Task Update_ReturnsOk_WithUpdatedQuiz()
        {
            var mockService = new Mock<IQuizService>();
            var updateDto = new QuizDTO.UpdateQuizDto { Title = "Updated" };
            var updated = new QuizDTO.QuizDto { QuizId = 200, CourseId = 12, Title = "Updated" };
            var expected = new ServiceResponse<(QuizDTO.QuizDto UpdatedQuiz, object OldValues)>((updated, new { Title = "Old" }));
            mockService.Setup(s => s.UpdateQuizAsync(200, It.IsAny<QuizDTO.UpdateQuizDto>()))
                       .ReturnsAsync(expected);

            var controller = CreateController(mockService);

            var result = await controller.Update(200, updateDto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var val = ok.Value!;
            var successProp = val.GetType().GetProperty("Success")!.GetValue(val);
            Assert.True((bool)successProp!);
            var dataProp = val.GetType().GetProperty("Data")!.GetValue(val) as QuizDTO.QuizDto;
            Assert.NotNull(dataProp);
            Assert.Equal(200, dataProp!.QuizId);
            Assert.Equal("Updated", dataProp!.Title);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WithSuccessTrue()
        {
            var mockService = new Mock<IQuizService>();
            var infoDto = new QuizDTO.QuizDto { QuizId = 300, CourseId = 10, Title = "Pop Quiz" };
            var expected = new ServiceResponse<(bool Deleted, object QuizInfo)>((true, infoDto));
            mockService.Setup(s => s.DeleteQuizAsync(300)).ReturnsAsync(expected);

            var controller = CreateController(mockService);

            var result = await controller.Delete(300);

            var ok = Assert.IsType<OkObjectResult>(result);
            var val = ok.Value!;
            var successProp = val.GetType().GetProperty("Success")!.GetValue(val);
            Assert.True((bool)successProp!);
            var dataProp = val.GetType().GetProperty("Data")!.GetValue(val);
            Assert.IsType<bool>(dataProp!);
            Assert.True((bool)dataProp!);
        }

        [Fact]
        public void Controller_HasAuthorizeAndApiControllerAttributes()
        {
            var type = typeof(QuizController);
            var hasAuthorize = type.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).Any();
            var hasApiController = type.GetCustomAttributes(typeof(ApiControllerAttribute), inherit: true).Any();
            Assert.True(hasAuthorize);
            Assert.True(hasApiController);
        }

        [Fact]
        public void Controller_HasRouteAttributeWithExpectedTemplate()
        {
            var type = typeof(QuizController);
            var routeAttr = type.GetCustomAttributes(typeof(RouteAttribute), inherit: true)
                                .Cast<RouteAttribute>()
                                .FirstOrDefault();
            Assert.NotNull(routeAttr);
            Assert.Equal("api/[controller]", routeAttr!.Template);
        }

        [Fact]
        public void Endpoints_HaveExpectedHttpMethodAttributes()
        {
            var type = typeof(QuizController);

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

            var getByCourse = type.GetMethod("GetByCourse");
            var getByCourseAttr = Assert.Single(getByCourse!.GetCustomAttributes(typeof(HttpGetAttribute), inherit: true));
            Assert.IsType<HttpGetAttribute>(getByCourseAttr);
            var getByCourseTemplate = ((HttpGetAttribute)getByCourseAttr).Template;
            Assert.Equal("course/{courseId}", getByCourseTemplate);
        }
    }
}