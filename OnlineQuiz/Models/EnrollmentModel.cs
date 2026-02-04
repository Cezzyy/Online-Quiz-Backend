using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class EnrollmentModel
    {
        [Key]
        public long EnrollmentId { get; set; }
        
        [Required]
        public long UserId { get; set; }
        
        [Required]
        public long CourseId { get; set; }
        
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        
        // Enrollment Status & Tracking
        [StringLength(20)]
        public string Status { get; set; } = "Active";
        
        public DateTime? CompletedAt { get; set; }
        
        public DateTime? DroppedAt { get; set; }
        
        [StringLength(500)]
        public string? DropReason { get; set; }
        
        // Grade Information
        [Column(TypeName = "decimal(5,2)")]
        public decimal? FinalGrade { get; set; }
        
        [StringLength(5)]
        public string? LetterGrade { get; set; }
        
        public bool IsPassed { get; set; } = false;
        
        // Progress Tracking
        public int QuizzesCompleted { get; set; } = 0;
        
        public int TotalQuizzes { get; set; } = 0;
        
        // Timestamps
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
        
        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;
    }
}