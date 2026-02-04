using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class AttemptAnswerModel
    {
        [Key]
        public long AttemptAnswerId { get; set; }
        
        [Required]
        public long AttemptId { get; set; }
        
        [Required]
        public long QuestionId { get; set; }
        
        public long? ChoiceId { get; set; }
        
        [Column("Free_Text")]
        public string? FreeText { get; set; }
        
        [Column("Is_Correct")]
        public bool? IsCorrect { get; set; }
        
        // Answer Metadata
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
        
        public int TimeSpentSeconds { get; set; } = 0;
        
        // Scoring
        [Column(TypeName = "decimal(5,2)")]
        public decimal? PointsEarned { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? PointsPossible { get; set; }
        
        // Feedback
        [StringLength(1000)]
        public string? Feedback { get; set; }
        
        public bool IsSkipped { get; set; } = false;
        
        // Navigation properties
        [ForeignKey("AttemptId")]
        public virtual AttemptModel Attempt { get; set; } = null!;
        
        [ForeignKey("QuestionId")]
        public virtual QuestionModel Question { get; set; } = null!;
        
        [ForeignKey("ChoiceId")]
        public virtual ChoiceModel? Choice { get; set; }
    }
}