using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineQuiz.Controllers;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Xunit;

namespace OnlineQuiz.Tests.Controllers
{
    public class ActivityLogControllerTests
    {
        private readonly Mock<IActivityLogService> _activityLogServiceMock;
        private readonly Mock<ILogger<ActivityLogController>> _loggerMock;
        private readonly ActivityLogController _controller;

        public ActivityLogControllerTests()
        {
            _activityLogServiceMock = new Mock<IActivityLogService>();
            _loggerMock = new Mock<ILogger<ActivityLogController>>();
            _controller = new ActivityLogController(_activityLogServiceMock.Object, _loggerMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetMyActivities_Success_ReturnsOkWithData()
        {
            var logs = new List<ActivityLogModel>
            {
                new ActivityLogModel
                {
                    ActivityLogId = 10,
                    UserId = 1,
                    User = new UserModel { FullName = "Test User", Email = "test@example.com" },
                    Action = "LOGIN",
                    Entity = "Auth",
                    Description = "Logged in",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _activityLogServiceMock
                .Setup(s => s.GetUserActivityLogsAsync(1, 1, 20))
                .ReturnsAsync(new ServiceResponse<IEnumerable<ActivityLogModel>>
                {
                    Success = true,
                    Data = logs,
                    Message = "Fetched"
                });

            var result = await _controller.GetMyActivities();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var success = GetProperty<bool>(ok.Value!, "success");
            var message = GetProperty<string>(ok.Value!, "message");
            Assert.True(success);
            Assert.Equal("Fetched", message);
            var dataObj = GetProperty<object>(ok.Value!, "data");
            var data = Assert.IsAssignableFrom<IEnumerable<ActivityLogDto>>(dataObj).ToList();
            Assert.Single(data);
            Assert.Equal(10, data[0].ActivityLogId);
            Assert.Equal("Test User", data[0].UserName);
            Assert.Equal("test@example.com", data[0].UserEmail);
        }

        [Fact]
        public async Task GetMyActivities_InvalidUser_ReturnsUnauthorized()
        {
            // Remove NameIdentifier claim
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "NoId User")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await _controller.GetMyActivities();
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetUserActivities_Success_ReturnsOk()
        {
            var logs = new List<ActivityLogModel>
            {
                new ActivityLogModel
                {
                    ActivityLogId = 11,
                    UserId = 2,
                    User = new UserModel { FullName = "Another User", Email = "another@example.com" },
                    Action = "CREATE",
                    Entity = "User",
                    EntityId = 2,
                    Description = "Created user",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _activityLogServiceMock
                .Setup(s => s.GetUserActivityLogsAsync(2, 1, 20))
                .ReturnsAsync(new ServiceResponse<IEnumerable<ActivityLogModel>>
                {
                    Success = true,
                    Data = logs,
                    Message = "Fetched"
                });

            var result = await _controller.GetUserActivities(2);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(GetProperty<bool>(ok.Value!, "success"));
            var dataObj = GetProperty<object>(ok.Value!, "data");
            var data = Assert.IsAssignableFrom<IEnumerable<ActivityLogDto>>(dataObj).ToList();
            Assert.Single(data);
            Assert.Equal("User", data[0].Entity);
            Assert.Equal(2, data[0].EntityId);
        }

        [Fact]
        public async Task GetEntityActivities_Success_ReturnsOk()
        {
            var logs = new List<ActivityLogModel>
            {
                new ActivityLogModel
                {
                    ActivityLogId = 12,
                    UserId = 3,
                    User = new UserModel { FullName = "Entity User", Email = "entity@example.com" },
                    Action = "UPDATE",
                    Entity = "Course",
                    EntityId = 5,
                    Description = "Updated course",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _activityLogServiceMock
                .Setup(s => s.GetEntityActivityLogsAsync("Course", 5, 1, 20))
                .ReturnsAsync(new ServiceResponse<IEnumerable<ActivityLogModel>>
                {
                    Success = true,
                    Data = logs,
                    Message = "Fetched"
                });

            var result = await _controller.GetEntityActivities("Course", 5);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(GetProperty<bool>(ok.Value!, "success"));
            var dataObj = GetProperty<object>(ok.Value!, "data");
            var data = Assert.IsAssignableFrom<IEnumerable<ActivityLogDto>>(dataObj).ToList();
            Assert.Single(data);
            Assert.Equal("Course", data[0].Entity);
            Assert.Equal(5, data[0].EntityId);
        }

        [Fact]
        public async Task GetAllActivities_Success_ReturnsDetailDto()
        {
            var logs = new List<ActivityLogModel>
            {
                new ActivityLogModel
                {
                    ActivityLogId = 13,
                    UserId = 4,
                    User = new UserModel { FullName = "Detail User", Email = "detail@example.com" },
                    Action = "DELETE",
                    Entity = "Quiz",
                    EntityId = 7,
                    Description = "Deleted quiz",
                    OldValues = "{ \"name\": \"Old Quiz\" }",
                    NewValues = null,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _activityLogServiceMock
                .Setup(s => s.GetAllActivityLogsAsync(1, 20))
                .ReturnsAsync(new ServiceResponse<IEnumerable<ActivityLogModel>>
                {
                    Success = true,
                    Data = logs,
                    Message = "Fetched"
                });

            var result = await _controller.GetAllActivities();
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True(GetProperty<bool>(ok.Value!, "success"));
            var dataObj = GetProperty<object>(ok.Value!, "data");
            var data = Assert.IsAssignableFrom<IEnumerable<ActivityLogDetailDto>>(dataObj).ToList();
            Assert.Single(data);
            Assert.Equal("{ \"name\": \"Old Quiz\" }", data[0].OldValues);
            Assert.Null(data[0].NewValues);
        }

        private static T? GetProperty<T>(object obj, string propertyPath)
        {
            object? current = obj;
            foreach (var name in propertyPath.Split('.'))
            {
                if (current == null) return default;
                var type = current.GetType();
                var prop = type.GetProperty(name);
                if (prop == null) return default;
                current = prop.GetValue(current);
            }
            return (T?)current;
        }
    }
}