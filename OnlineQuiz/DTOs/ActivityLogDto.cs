using System.ComponentModel.DataAnnotations;

namespace OnlineQuiz.DTOs
{
    public class ActivityLogDto
    {
        public long ActivityLogId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public long? EntityId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Request Information
        public string? HttpMethod { get; set; }
        public string? RequestPath { get; set; }
        
        // Response Information
        public int? StatusCode { get; set; }
        public int? ResponseTimeMs { get; set; }
        
        // Error Information
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Severity Level
        public string Severity { get; set; } = "Info";
    }

    public class ActivityLogDetailDto : ActivityLogDto
    {
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }

    public class ActivityLogFilterDto
    {
        public long? UserId { get; set; }
        public string? Action { get; set; }
        public string? Entity { get; set; }
        public long? EntityId { get; set; }
        public string? Severity { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ActivityLogSummaryDto
    {
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime LastActivity { get; set; }
    }
}