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
        
        // Navigation properties
        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;
        
        public virtual ICollection<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
        public virtual ICollection<AttemptModel> Attempts { get; set; } = new List<AttemptModel>();
    }
}