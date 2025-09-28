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
        
        // Navigation properties
        [ForeignKey("AttemptId")]
        public virtual AttemptModel Attempt { get; set; } = null!;
        
        [ForeignKey("QuestionId")]
        public virtual QuestionModel Question { get; set; } = null!;
        
        [ForeignKey("ChoiceId")]
        public virtual ChoiceModel? Choice { get; set; }
    }
}