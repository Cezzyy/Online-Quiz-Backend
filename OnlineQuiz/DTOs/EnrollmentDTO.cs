using System.ComponentModel.DataAnnotations;

namespace OnlineQuiz.DTOs
{
    public class EnrollmentDTO
    {
        public class EnrollmentDto
        {
            public long EnrollmentId { get; set; }
            public long CourseId { get; set; }
            public long UserId { get; set; }
            public string? CourseName { get; set; }
            public string? CourseCode { get; set; }
            public string? StudentName { get; set; }
            public string? StudentEmail { get; set; }
            public DateTime EnrolledAt { get; set; }
            
            // Enrollment Status & Tracking
            public string Status { get; set; } = "Active";
            public DateTime? CompletedAt { get; set; }
            public DateTime? DroppedAt { get; set; }
            public string? DropReason { get; set; }
            
            // Grade Information
            public decimal? FinalGrade { get; set; }
            public string? LetterGrade { get; set; }
            public bool IsPassed { get; set; }
            
            // Progress Tracking
            public int QuizzesCompleted { get; set; }
            public int TotalQuizzes { get; set; }
            
            // Timestamps
            public DateTime UpdatedAt { get; set; }
        }

        public class CreateEnrollmentDto
        {
            [Required]
            public long CourseId { get; set; }
            
            [Required]
            public long UserId { get; set; }
        }

        public class EnrollStudentDto
        {
            [Required]
            public long UserId { get; set; }
        }

        public class UpdateEnrollmentDto
        {
            public string? Status { get; set; }
            public string? DropReason { get; set; }
        }

        public class UpdateEnrollmentGradeDto
        {
            public decimal? FinalGrade { get; set; }
            public string? LetterGrade { get; set; }
            public bool? IsPassed { get; set; }
        }
    }
}
