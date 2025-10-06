using OnlineQuiz.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineQuiz.DTOs
{
    public class CourseDTO
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("Instructor_UserId")]
        public long InstructorUserId { get; set; }

        // Navigation properties
        [ForeignKey("InstructorUserId")]
        public virtual TeacherModel Instructor { get; set; } = null!;
    }
}
