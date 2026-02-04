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
        public virtual InstructorModel Instructor { get; set; } = null!;

        public class CreateCourseDto
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public long InstructorUserId { get; set; }
            public string Status { get; set; } = "Active";
            public string? Category { get; set; }
            public string? Description { get; set; }
            public string? Semester { get; set; }
            public int? AcademicYear { get; set; }
            public int? Units { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool IsPublished { get; set; } = false;
        }

        public class UpdateCourseDto
        {
            public string? Code { get; set; }
            public string? Name { get; set; }
            public long? InstructorUserId { get; set; }
            public string? Status { get; set; }
            public string? Category { get; set; }
            public string? Description { get; set; }
            public string? Semester { get; set; }
            public int? AcademicYear { get; set; }
            public int? Units { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool? IsPublished { get; set; }
        }

        public class CourseDto
        {
            public long CourseId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public long InstructorUserId { get; set; }
            public string? InstructorName { get; set; }
            public string Status { get; set; } = string.Empty;
            public string? Category { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public long CreatedBy { get; set; }
            public string? CreatedByName { get; set; }
            public string? Description { get; set; }
            public string? Semester { get; set; }
            public int? AcademicYear { get; set; }
            public int? Units { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool IsPublished { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime? DeletedAt { get; set; }
        }
    }
}
