using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class QuizModel
    {
        [Key]
        public long QuizId { get; set; }
        
        [Required]
        public long CourseId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Column("Due_At")]
        public DateTime? DueAt { get; set; }
        
        [Column("Time_Limit_Minutes")]
        public int? TimeLimitMinutes { get; set; }
        
        [Column("Is_Published")]
        public bool IsPublished { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Quiz Configuration
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [StringLength(1000)]
        public string? Instructions { get; set; }
        
        // Attempt Configuration
        public int? MaxAttempts { get; set; }
        
        public bool ShuffleQuestions { get; set; } = false;
        
        public bool ShuffleChoices { get; set; } = false;
        
        // Availability
        public DateTime? AvailableFrom { get; set; }
        
        public DateTime? AvailableUntil { get; set; }
        
        // Grading Configuration
        [Column(TypeName = "decimal(5,2)")]
        public decimal PassingScore { get; set; } = 60.00m;
        
        public bool ShowCorrectAnswers { get; set; } = true;
        
        public bool ShowScoreImmediately { get; set; } = true;
        
        // Soft Delete
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;
        
        public virtual ICollection<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
        public virtual ICollection<AttemptModel> Attempts { get; set; } = new List<AttemptModel>();
    }
}