using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class AttemptModel
    {
        [Key]
        public long AttemptId { get; set; }
        
        [Required]
        public long QuizId { get; set; }
        
        [Required]
        public long UserId { get; set; }
        
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? SubmittedAt { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Score { get; set; } = 0;
        
        [Column("Time_Spent_Seconds")]
        public int? TimeSpentSeconds { get; set; }
        
        // Attempt Status
        [StringLength(20)]
        public string Status { get; set; } = "InProgress";
        
        // Grading Information
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Percentage { get; set; }
        
        public bool IsPassed { get; set; } = false;
        
        // Feedback
        [StringLength(2000)]
        public string? InstructorFeedback { get; set; }
        
        public long? GradedBy { get; set; }
        
        public DateTime? GradedAt { get; set; }
        
        // Integrity Tracking
        public int TabSwitchCount { get; set; } = 0;
        
        public bool FlaggedForReview { get; set; } = false;
        
        [StringLength(1000)]
        public string? ReviewNotes { get; set; }
        
        // Timestamps
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("QuizId")]
        public virtual QuizModel Quiz { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
        
        public virtual ICollection<AttemptAnswerModel> AttemptAnswers { get; set; } = new List<AttemptAnswerModel>();
    }
}