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
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
        
        [ForeignKey("CourseId")]
        public virtual CourseModel Course { get; set; } = null!;
    }
}