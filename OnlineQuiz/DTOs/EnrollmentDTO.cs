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
    }
}
