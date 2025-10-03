using OnlineQuiz.DTOs;
using Xunit;

namespace OnlineQuiz.Tests.DTOs
{
    public class ActivityLogDtoTests
    {
        [Fact]
        public void ActivityLogDto_Defaults_AreInitialized()
        {
            var dto = new ActivityLogDto();
            Assert.Equal(0L, dto.ActivityLogId);
            Assert.Equal(0L, dto.UserId);
            Assert.Equal(string.Empty, dto.UserName);
            Assert.Equal(string.Empty, dto.UserEmail);
            Assert.Equal(string.Empty, dto.Action);
            Assert.Equal(string.Empty, dto.Entity);
            Assert.Null(dto.EntityId);
            Assert.Null(dto.Description);
            Assert.Equal(default, dto.CreatedAt);
        }

        [Fact]
        public void ActivityLogDto_Assignments_Work()
        {
            var now = DateTime.UtcNow;
            var dto = new ActivityLogDto
            {
                ActivityLogId = 10,
                UserId = 5,
                UserName = "Tester",
                UserEmail = "tester@example.com",
                Action = "Create",
                Entity = "Quiz",
                EntityId = 123,
                Description = "Created a quiz",
                CreatedAt = now
            };

            Assert.Equal(10L, dto.ActivityLogId);
            Assert.Equal(5L, dto.UserId);
            Assert.Equal("Tester", dto.UserName);
            Assert.Equal("tester@example.com", dto.UserEmail);
            Assert.Equal("Create", dto.Action);
            Assert.Equal("Quiz", dto.Entity);
            Assert.Equal(123L, dto.EntityId);
            Assert.Equal("Created a quiz", dto.Description);
            Assert.Equal(now, dto.CreatedAt);
        }

        [Fact]
        public void ActivityLogDetailDto_Defaults_AreInitialized()
        {
            var dto = new ActivityLogDetailDto();
            Assert.Null(dto.OldValues);
            Assert.Null(dto.NewValues);
            // Inherited defaults
            Assert.Equal(0L, dto.ActivityLogId);
            Assert.Equal(0L, dto.UserId);
            Assert.Equal(string.Empty, dto.UserName);
            Assert.Equal(string.Empty, dto.UserEmail);
            Assert.Equal(string.Empty, dto.Action);
            Assert.Equal(string.Empty, dto.Entity);
        }

        [Fact]
        public void ActivityLogFilterDto_Defaults_AreInitialized()
        {
            var dto = new ActivityLogFilterDto();
            Assert.Null(dto.UserId);
            Assert.Null(dto.Action);
            Assert.Null(dto.Entity);
            Assert.Null(dto.EntityId);
            Assert.Null(dto.FromDate);
            Assert.Null(dto.ToDate);
            Assert.Equal(1, dto.Page);
            Assert.Equal(20, dto.PageSize);
        }

        [Fact]
        public void ActivityLogSummaryDto_Defaults_AreInitialized()
        {
            var dto = new ActivityLogSummaryDto();
            Assert.Equal(string.Empty, dto.Action);
            Assert.Equal(string.Empty, dto.Entity);
            Assert.Equal(0, dto.Count);
            Assert.Equal(default, dto.LastActivity);
        }
    }
}