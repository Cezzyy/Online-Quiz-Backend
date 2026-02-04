using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class UserModel
    {
        [Key]
        public long UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [StringLength(60)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string Status { get; set; } = "Active";
        
        [StringLength(30)]
        public string? ContactNumber { get; set; }
        
        [StringLength(30)]
        public string? EmergencyContactNumber { get; set; }
        
        [StringLength(100)]
        public string? EmergencyContactPersonName { get; set; }
        
        [StringLength(1000)]
        public string? Bio { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
        
        public long? DeletedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<UserRoleModel> UserRoles { get; set; } = new List<UserRoleModel>();
        public virtual InstructorModel? Instructor { get; set; }
        public virtual StudentModel? Student { get; set; }
        public virtual ICollection<EnrollmentModel> Enrollments { get; set; } = new List<EnrollmentModel>();
        public virtual ICollection<AttemptModel> Attempts { get; set; } = new List<AttemptModel>();
        public virtual ICollection<NotificationModel> Notifications { get; set; } = new List<NotificationModel>();
        public virtual ICollection<ExportImportLogModel> ExportImportLogs { get; set; } = new List<ExportImportLogModel>();
        public virtual ICollection<ActivityLogModel> ActivityLogs { get; set; } = new List<ActivityLogModel>();
    }
}