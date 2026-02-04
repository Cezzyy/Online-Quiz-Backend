using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly OnlineQuizDbContext _context;
        private readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(OnlineQuizDbContext context, ILogger<ActivityLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActivityAsync(long userId, string action, string entity, long? entityId = null, string? description = null, object? oldValues = null, object? newValues = null)
        {
            try
            {
                var activityLog = new ActivityLogModel
                {
                    UserId = userId,
                    Action = action.ToUpper(),
                    Entity = entity,
                    EntityId = entityId,
                    Description = description,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    CreatedAt = DateTime.UtcNow,
                    Severity = "Info" // Default severity
                };

                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Activity logged: User {UserId} performed {Action} on {Entity} {EntityId}", 
                    userId, action, entity, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log activity for user {UserId}: {Action} on {Entity} {EntityId}", 
                    userId, action, entity, entityId);
                // Don't throw - logging failures shouldn't break the main operation
            }
        }

        public async Task LogUserActionAsync(long userId, string action, string description)
        {
            await LogActivityAsync(userId, action, "System", null, description);
        }

        public async Task LogEntityActionAsync(long userId, string action, string entity, long entityId, string description, object? oldValues = null, object? newValues = null)
        {
            await LogActivityAsync(userId, action, entity, entityId, description, oldValues, newValues);
        }

        public async Task LogHttpRequestAsync(long userId, string action, string entity, long? entityId, string? description, 
            string? httpMethod, string? requestPath, int? statusCode, int? responseTimeMs, string? errorCode = null, string? errorMessage = null, string severity = "Info")
        {
            try
            {
                var activityLog = new ActivityLogModel
                {
                    UserId = userId,
                    Action = action.ToUpper(),
                    Entity = entity,
                    EntityId = entityId,
                    Description = description,
                    HttpMethod = httpMethod,
                    RequestPath = requestPath,
                    StatusCode = statusCode,
                    ResponseTimeMs = responseTimeMs,
                    ErrorCode = errorCode,
                    ErrorMessage = errorMessage,
                    Severity = severity,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ActivityLogs.Add(activityLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("HTTP request logged: {HttpMethod} {RequestPath} - Status {StatusCode} - User {UserId}", 
                    httpMethod, requestPath, statusCode, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log HTTP request for user {UserId}", userId);
            }
        }

        public async Task<ServiceResponse<IEnumerable<ActivityLogModel>>> GetUserActivityLogsAsync(long userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var logs = await _context.ActivityLogs
                    .Where(a => a.UserId == userId)
                    .Include(a => a.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new ServiceResponse<IEnumerable<ActivityLogModel>>(logs)
                {
                    Message = $"Retrieved {logs.Count} activity logs for user {userId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity logs for user {UserId}", userId);
                return new ServiceResponse<IEnumerable<ActivityLogModel>>($"Error retrieving activity logs: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<ActivityLogModel>>> GetEntityActivityLogsAsync(string entity, long entityId, int page = 1, int pageSize = 20)
        {
            try
            {
                var logs = await _context.ActivityLogs
                    .Where(a => a.Entity == entity && a.EntityId == entityId)
                    .Include(a => a.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new ServiceResponse<IEnumerable<ActivityLogModel>>(logs)
                {
                    Message = $"Retrieved {logs.Count} activity logs for {entity} {entityId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity logs for {Entity} {EntityId}", entity, entityId);
                return new ServiceResponse<IEnumerable<ActivityLogModel>>($"Error retrieving activity logs: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<IEnumerable<ActivityLogModel>>> GetAllActivityLogsAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                var logs = await _context.ActivityLogs
                    .Include(a => a.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new ServiceResponse<IEnumerable<ActivityLogModel>>(logs)
                {
                    Message = $"Retrieved {logs.Count} activity logs"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activity logs");
                return new ServiceResponse<IEnumerable<ActivityLogModel>>($"Error retrieving activity logs: {ex.Message}");
            }
        }
    }
}