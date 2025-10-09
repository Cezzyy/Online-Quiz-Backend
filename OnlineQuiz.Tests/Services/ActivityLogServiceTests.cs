using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineQuiz.Data;
using OnlineQuiz.Models;
using OnlineQuiz.Services;
using Xunit;

namespace OnlineQuiz.Tests.Services
{
    public class ActivityLogServiceTests
    {
        private OnlineQuizDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase(databaseName: $"ActivityLogServiceTests_{Guid.NewGuid()}")
                .Options;
            return new OnlineQuizDbContext(options);
        }

        [Fact]
        public async Task LogActivityAsync_AddsLogAndSaves()
        {
            // Arrange
            var context = CreateDbContext();
            var loggerMock = new Mock<ILogger<ActivityLogService>>();
            var service = new ActivityLogService(context, loggerMock.Object);

           // Act
           await service.LogActivityAsync(1, "TestAction", "System", description: "Details");

            // Assert
            var log = await context.ActivityLogs.FirstOrDefaultAsync(al => al.UserId == 1);
            Assert.NotNull(log);
            Assert.Equal(1, log!.UserId);
            Assert.Equal("TESTACTION", log!.Action);
            Assert.Equal("System", log!.Entity);
            Assert.Equal("Details", log!.Description);
        }

        [Fact]
        public async Task LogActivityAsync_HandlesNullDetails()
        {
            var context = CreateDbContext();
            var loggerMock = new Mock<ILogger<ActivityLogService>>();
            var service = new ActivityLogService(context, loggerMock.Object);

            await service.LogActivityAsync(2, "Login", "System");

            var log = await context.ActivityLogs.FirstOrDefaultAsync(al => al.UserId == 2);
            Assert.NotNull(log);
            Assert.Equal("LOGIN", log!.Action);
            Assert.Null(log!.Description);
        }
    }
}