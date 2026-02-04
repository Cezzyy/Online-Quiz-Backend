using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class ActivityLogModel
    {
        [Key]
        public long ActivityLogId { get; set; }
        
        [Required]
        public long UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN, LOGOUT, etc.
        
        [Required]
        [StringLength(50)]
        public string Entity { get; set; } = string.Empty; // User, Course, Quiz, Enrollment, etc.
        
        public long? EntityId { get; set; } // ID of the affected entity (nullable for system actions)
        
        [StringLength(500)]
        public string? Description { get; set; } // Human-readable description of the action
        
        [Column(TypeName = "nvarchar(max)")]
        public string? OldValues { get; set; } // JSON of old values (for updates)
        
        [Column(TypeName = "nvarchar(max)")]
        public string? NewValues { get; set; } // JSON of new values (for creates/updates)
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Request Information
        [StringLength(10)]
        public string? HttpMethod { get; set; }
        
        [StringLength(500)]
        public string? RequestPath { get; set; }
        
        // Response Information
        public int? StatusCode { get; set; }
        
        public int? ResponseTimeMs { get; set; }
        
        // Error Information
        [StringLength(100)]
        public string? ErrorCode { get; set; }
        
        [Column(TypeName = "nvarchar(max)")]
        public string? ErrorMessage { get; set; }
        
        // Severity Level
        [StringLength(20)]
        public string Severity { get; set; } = "Info";
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
    }
}