using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.Models
{
    public class TeacherModel
    {
        [Key]
        public long UserId { get; set; }
        
        [StringLength(120)]
        public string? Department { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; } = null!;
        
        public virtual ICollection<CourseModel> Courses { get; set; } = new List<CourseModel>();
    }
}