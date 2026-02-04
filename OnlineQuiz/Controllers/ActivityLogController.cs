using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using System.Security.Claims;

namespace OnlineQuiz.Controllers
{
    [ApiController]
    [Route("api/activity-logs")]
    [Authorize]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<ActivityLogController> _logger;

        public ActivityLogController(IActivityLogService activityLogService, ILogger<ActivityLogController> logger)
        {
            _activityLogService = activityLogService;
            _logger = logger;
        }

        /// <summary>
        /// Get activity logs for the current user
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetMyActivities([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdClaim, out long userId))
            {
                return Unauthorized("Invalid user token");
            }

            var result = await _activityLogService.GetUserActivityLogsAsync(userId, page, pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var activityDtos = result.Data?.Select(a => new ActivityLogDto
            {
                ActivityLogId = a.ActivityLogId,
                UserId = a.UserId,
                UserName = a.User?.FullName ?? "Unknown",
                UserEmail = a.User?.Email ?? "Unknown",
                Action = a.Action,
                Entity = a.Entity,
                EntityId = a.EntityId,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                HttpMethod = a.HttpMethod,
                RequestPath = a.RequestPath,
                StatusCode = a.StatusCode,
                ResponseTimeMs = a.ResponseTimeMs,
                ErrorCode = a.ErrorCode,
                ErrorMessage = a.ErrorMessage,
                Severity = a.Severity
            });

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = activityDtos,
                pagination = new
                {
                    page,
                    pageSize,
                    hasMore = activityDtos?.Count() == pageSize
                }
            });
        }

        /// <summary>
        /// Get activity logs for a specific user (Admin only)
        /// </summary>
        [HttpGet("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetUserActivities(long userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _activityLogService.GetUserActivityLogsAsync(userId, page, pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var activityDtos = result.Data?.Select(a => new ActivityLogDto
            {
                ActivityLogId = a.ActivityLogId,
                UserId = a.UserId,
                UserName = a.User?.FullName ?? "Unknown",
                UserEmail = a.User?.Email ?? "Unknown",
                Action = a.Action,
                Entity = a.Entity,
                EntityId = a.EntityId,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                HttpMethod = a.HttpMethod,
                RequestPath = a.RequestPath,
                StatusCode = a.StatusCode,
                ResponseTimeMs = a.ResponseTimeMs,
                ErrorCode = a.ErrorCode,
                ErrorMessage = a.ErrorMessage,
                Severity = a.Severity
            });

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = activityDtos,
                pagination = new
                {
                    page,
                    pageSize,
                    hasMore = activityDtos?.Count() == pageSize
                }
            });
        }

        /// <summary>
        /// Get activity logs for a specific entity (Admin only)
        /// </summary>
        [HttpGet("entities/{entity}/{entityId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetEntityActivities(string entity, long entityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _activityLogService.GetEntityActivityLogsAsync(entity, entityId, page, pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var activityDtos = result.Data?.Select(a => new ActivityLogDto
            {
                ActivityLogId = a.ActivityLogId,
                UserId = a.UserId,
                UserName = a.User?.FullName ?? "Unknown",
                UserEmail = a.User?.Email ?? "Unknown",
                Action = a.Action,
                Entity = a.Entity,
                EntityId = a.EntityId,
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                HttpMethod = a.HttpMethod,
                RequestPath = a.RequestPath,
                StatusCode = a.StatusCode,
                ResponseTimeMs = a.ResponseTimeMs,
                ErrorCode = a.ErrorCode,
                ErrorMessage = a.ErrorMessage,
                Severity = a.Severity
            });

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = activityDtos,
                pagination = new
                {
                    page,
                    pageSize,
                    hasMore = activityDtos?.Count() == pageSize
                }
            });
        }

        /// <summary>
        /// Get all activity logs (Admin only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ActivityLogDetailDto>>> GetAllActivities([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _activityLogService.GetAllActivityLogsAsync(page, pageSize);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            var activityDtos = result.Data?.Select(a => new ActivityLogDetailDto
            {
                ActivityLogId = a.ActivityLogId,
                UserId = a.UserId,
                UserName = a.User?.FullName ?? "Unknown",
                UserEmail = a.User?.Email ?? "Unknown",
                Action = a.Action,
                Entity = a.Entity,
                EntityId = a.EntityId,
                Description = a.Description,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                CreatedAt = a.CreatedAt,
                HttpMethod = a.HttpMethod,
                RequestPath = a.RequestPath,
                StatusCode = a.StatusCode,
                ResponseTimeMs = a.ResponseTimeMs,
                ErrorCode = a.ErrorCode,
                ErrorMessage = a.ErrorMessage,
                Severity = a.Severity
            });

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = activityDtos,
                pagination = new
                {
                    page,
                    pageSize,
                    hasMore = activityDtos?.Count() == pageSize
                }
            });
        }
    }
}