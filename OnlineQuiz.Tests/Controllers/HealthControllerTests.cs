using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using OnlineQuiz.Controllers;
using OnlineQuiz.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OnlineQuiz.Tests.Controllers
{
    public class HealthControllerTests
    {
        private readonly Mock<OnlineQuizDbContext> _mockContext;
        private readonly HealthController _controller;

        public HealthControllerTests()
        {
            // Create mock for DbContext
            var options = new DbContextOptionsBuilder<OnlineQuizDbContext>()
                .UseInMemoryDatabase(databaseName: "TestHealthDb")
                .Options;
            
            _mockContext = new Mock<OnlineQuizDbContext>(options);
            _controller = new HealthController(_mockContext.Object);
        }

        [Fact]
        public async Task HealthCheck_DatabaseConnected_ReturnsOk()
        {
            // Arrange
            var mockDatabase = new Mock<DatabaseFacade>(_mockContext.Object);
            mockDatabase.Setup(d => d.CanConnectAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

            // Act
            var result = await _controller.HealthCheck();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(okResult.Value);
            Assert.Equal("healthy", GetProperty<string>(okResult.Value!, "status"));
            Assert.Equal("healthy", GetProperty<string>(okResult.Value!, "database.status"));
            Assert.True(GetProperty<bool>(okResult.Value!, "database.connected"));
        }

        [Fact]
        public async Task HealthCheck_DatabaseDisconnected_ReturnsServiceUnavailable()
        {
            // Arrange
            var mockDatabase = new Mock<DatabaseFacade>(_mockContext.Object);
            mockDatabase.Setup(d => d.CanConnectAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

            // Act
            var result = await _controller.HealthCheck();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);
            Assert.NotNull(statusCodeResult.Value);
            Assert.Equal("unhealthy", GetProperty<string>(statusCodeResult.Value!, "status"));
            Assert.Equal("unhealthy", GetProperty<string>(statusCodeResult.Value!, "database.status"));
            Assert.False(GetProperty<bool>(statusCodeResult.Value!, "database.connected"));
        }

        [Fact]
        public async Task HealthCheck_DatabaseThrowsException_ReturnsServiceUnavailable()
        {
            // Arrange
            var mockDatabase = new Mock<DatabaseFacade>(_mockContext.Object);
            mockDatabase.Setup(d => d.CanConnectAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection error"));
            _mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

            // Act
            var result = await _controller.HealthCheck();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);
            Assert.NotNull(statusCodeResult.Value);
            Assert.Equal("unhealthy", GetProperty<string>(statusCodeResult.Value!, "status"));
            Assert.Equal("unhealthy", GetProperty<string>(statusCodeResult.Value!, "database.status"));
            Assert.False(GetProperty<bool>(statusCodeResult.Value!, "database.connected"));
            Assert.Equal("Database connection error", GetProperty<string>(statusCodeResult.Value!, "database.error"));
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
            return current is T t ? t : default;
        }
    }
}