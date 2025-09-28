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
        
        // Navigation properties
        [ForeignKey("QuizId")]
        public virtual QuizModel Quiz { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
        
        public virtual ICollection<AttemptAnswerModel> AttemptAnswers { get; set; } = new List<AttemptAnswerModel>();
    }
}