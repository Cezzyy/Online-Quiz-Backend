using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class CourseModel
    {
        [Key]
        public long CourseId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Column("Instructor_UserId")]
        public long InstructorUserId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive, Archived
        
        [StringLength(50)]
        public string? Category { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public long CreatedBy { get; set; } // Admin who created the course
        
        // Course Details
        [StringLength(2000)]
        public string? Description { get; set; }
        
        // ACLC Academic Information
        [StringLength(50)]
        public string? Semester { get; set; }
        
        public int? AcademicYear { get; set; }
        
        public int? Units { get; set; }
        
        // Course Dates
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        // Visibility
        public bool IsPublished { get; set; } = false;
        
        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("InstructorUserId")]
        public virtual InstructorModel Instructor { get; set; } = null!;
        
        [ForeignKey("CreatedBy")]
        public virtual UserModel Creator { get; set; } = null!;
        
        public virtual ICollection<EnrollmentModel> Enrollments { get; set; } = new List<EnrollmentModel>();
        public virtual ICollection<QuizModel> Quizzes { get; set; } = new List<QuizModel>();
        public virtual ICollection<NotificationModel> Notifications { get; set; } = new List<NotificationModel>();
    }
}