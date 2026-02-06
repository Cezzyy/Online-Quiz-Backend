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
            [Required(ErrorMessage = "Course code is required")]
            [StringLength(20, MinimumLength = 3, ErrorMessage = "Course code must be between 3 and 20 characters")]
            public string Code { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Course name is required")]
            [StringLength(200, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 200 characters")]
            public string Name { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Instructor is required")]
            public long InstructorUserId { get; set; }
            
            [StringLength(50)]
            public string Status { get; set; } = "Active";
            
            [StringLength(100)]
            public string? Category { get; set; }
            
            [StringLength(2000)]
            public string? Description { get; set; }
            
            [StringLength(50)]
            public string? Semester { get; set; }
            
            [Range(2000, 2100, ErrorMessage = "Academic year must be between 2000 and 2100")]
            public int? AcademicYear { get; set; }
            
            [Range(1, 10, ErrorMessage = "Units must be between 1 and 10")]
            public int? Units { get; set; }
            
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool IsPublished { get; set; } = false;
        }

        public class UpdateCourseDto
        {
            [StringLength(20, MinimumLength = 3, ErrorMessage = "Course code must be between 3 and 20 characters")]
            public string? Code { get; set; }
            
            [StringLength(200, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 200 characters")]
            public string? Name { get; set; }
            
            public long? InstructorUserId { get; set; }
            
            [StringLength(50)]
            public string? Status { get; set; }
            
            [StringLength(100)]
            public string? Category { get; set; }
            
            [StringLength(2000)]
            public string? Description { get; set; }
            
            [StringLength(50)]
            public string? Semester { get; set; }
            
            [Range(2000, 2100, ErrorMessage = "Academic year must be between 2000 and 2100")]
            public int? AcademicYear { get; set; }
            
            [Range(1, 10, ErrorMessage = "Units must be between 1 and 10")]
            public int? Units { get; set; }
            
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool? IsPublished { get; set; }
        }

        public class UpdateCourseDto
        {
            [StringLength(20, MinimumLength = 3, ErrorMessage = "Course code must be between 3 and 20 characters")]
            public string? Code { get; set; }
            
            [StringLength(200, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 200 characters")]
            public string? Name { get; set; }
            
            public long? InstructorUserId { get; set; }
            
            [StringLength(50)]
            public string? Status { get; set; }
            
            [StringLength(100)]
            public string? Category { get; set; }
            
            [StringLength(2000)]
            public string? Description { get; set; }
            
            [StringLength(50)]
            public string? Semester { get; set; }
            
            [Range(2000, 2100, ErrorMessage = "Academic year must be between 2000 and 2100")]
            public int? AcademicYear { get; set; }
            
            [Range(1, 10, ErrorMessage = "Units must be between 1 and 10")]
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
            
            // Statistics
            public int EnrollmentCount { get; set; }
            public int QuizCount { get; set; }
        }

        public class CourseFilterDto
        {
            public string? SearchTerm { get; set; }
            public string? Status { get; set; }
            public string? Category { get; set; }
            public long? InstructorUserId { get; set; }
            public string? Semester { get; set; }
            public int? AcademicYear { get; set; }
            public bool? IsPublished { get; set; }
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string SortBy { get; set; } = "CreatedAt";
            public string SortOrder { get; set; } = "desc";
        }

        public class PagedCoursesDto
        {
            public IEnumerable<CourseDto> Courses { get; set; } = new List<CourseDto>();
            public int TotalCount { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public bool HasPreviousPage { get; set; }
            public bool HasNextPage { get; set; }
        }

        public class CourseStatisticsDto
        {
            public long CourseId { get; set; }
            public string CourseName { get; set; } = string.Empty;
            public string CourseCode { get; set; } = string.Empty;
            public int TotalEnrollments { get; set; }
            public int ActiveEnrollments { get; set; }
            public int CompletedEnrollments { get; set; }
            public int DroppedEnrollments { get; set; }
            public int TotalQuizzes { get; set; }
            public int PublishedQuizzes { get; set; }
            public decimal AverageGrade { get; set; }
            public decimal PassRate { get; set; }
            public DateTime? LastActivityDate { get; set; }
        }
    }
}
