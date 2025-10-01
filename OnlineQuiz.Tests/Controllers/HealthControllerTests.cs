using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            _mockContext.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.HealthCheck();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            
            dynamic healthResponse = okResult.Value;
            Assert.Equal("healthy", healthResponse.status);
            Assert.Equal("healthy", healthResponse.database.status);
            Assert.True(healthResponse.database.connected);
        }

        [Fact]
        public async Task HealthCheck_DatabaseDisconnected_ReturnsServiceUnavailable()
        {
            // Arrange
            _mockContext.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.HealthCheck();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);
            
            dynamic healthResponse = statusCodeResult.Value;
            Assert.Equal("unhealthy", healthResponse.status);
            Assert.Equal("unhealthy", healthResponse.database.status);
            Assert.False(healthResponse.database.connected);
        }

        [Fact]
        public async Task HealthCheck_DatabaseThrowsException_ReturnsServiceUnavailable()
        {
            // Arrange
            _mockContext.Setup(c => c.Database.CanConnectAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection error"));

            // Act
            var result = await _controller.HealthCheck();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusCodeResult.StatusCode);
            
            dynamic healthResponse = statusCodeResult.Value;
            Assert.Equal("unhealthy", healthResponse.status);
            Assert.Equal("unhealthy", healthResponse.database.status);
            Assert.False(healthResponse.database.connected);
            Assert.Equal("Database connection error", healthResponse.database.error);
        }
    }
}