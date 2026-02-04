using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class ChoiceModel
    {
        [Key]
        public long ChoiceId { get; set; }
        
        [Required]
        public long QuestionId { get; set; }
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        [Column("Is_Correct")]
        public bool IsCorrect { get; set; } = false;
        
        // Choice Ordering
        public int OrderIndex { get; set; } = 0;
        
        // Choice Metadata
        [StringLength(500)]
        public string? Explanation { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("QuestionId")]
        public virtual QuestionModel Question { get; set; } = null!;
        
        public virtual ICollection<AttemptAnswerModel> AttemptAnswers { get; set; } = new List<AttemptAnswerModel>();
    }
}