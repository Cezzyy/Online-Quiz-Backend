using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class StudentModel
    {
        [Key]
        public long UserId { get; set; }
        
        [Required]
        [StringLength(25)]
        public string StudentNumber { get; set; } = string.Empty;
        
        [Column("Year_Level")]
        public int? YearLevel { get; set; }
        
        [StringLength(60)]
        public string? Section { get; set; }
        
        [StringLength(120)]
        public string? Course { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
    }
}