using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IServices
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(long userId, string action, string entity, long? entityId = null, string? description = null, object? oldValues = null, object? newValues = null);
        Task LogUserActionAsync(long userId, string action, string description);
        Task LogEntityActionAsync(long userId, string action, string entity, long entityId, string description, object? oldValues = null, object? newValues = null);
        Task<ServiceResponse<IEnumerable<ActivityLogModel>>> GetUserActivityLogsAsync(long userId, int page = 1, int pageSize = 20);
        Task<ServiceResponse<IEnumerable<ActivityLogModel>>> GetEntityActivityLogsAsync(string entity, long entityId, int page = 1, int pageSize = 20);
        Task<ServiceResponse<IEnumerable<ActivityLogModel>>> GetAllActivityLogsAsync(int page = 1, int pageSize = 20);
    }
}