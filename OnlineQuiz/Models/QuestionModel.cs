using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class QuestionModel
    {
        [Key]
        public long QuestionId { get; set; }
        
        [Required]
        public long QuizId { get; set; }
        
        [Required]
        [StringLength(16)]
        public string Type { get; set; } = string.Empty; // Single, Multiple, Text
        
        [Required]
        public string Body { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal Points { get; set; } = 1;
        
        [Column("Sort_Order")]
        public int SortOrder { get; set; } = 1;
        
        // Navigation properties
        [ForeignKey("QuizId")]
        public virtual QuizModel Quiz { get; set; } = null!;
        
        public virtual ICollection<ChoiceModel> Choices { get; set; } = new List<ChoiceModel>();
        public virtual ICollection<AttemptAnswerModel> AttemptAnswers { get; set; } = new List<AttemptAnswerModel>();
    }
}