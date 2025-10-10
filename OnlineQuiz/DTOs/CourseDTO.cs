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

        public class CreateCourseDto
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public long InstructorUserId { get; set; }
        }

        public class UpdateCourseDto
        {
            public string? Code { get; set; }
            public string? Name { get; set; }
            public long? InstructorUserId { get; set; }
        }

        public class CourseDto
        {
            public long CourseId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public long InstructorUserId { get; set; }
            public string? InstructorName { get; set; }
        }
    }
}
