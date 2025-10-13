using OnlineQuiz.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineQuiz.DTOs
{
    public class QuizDTO
    {
        public class CreateQuizDto
        {
            [Required]
            public long CourseId { get; set; }
            
            [Required]
            [StringLength(200)]
            public string Title { get; set; } = string.Empty;
            
            public DateTime? DueAt { get; set; }
            
            public int? TimeLimitMinutes { get; set; }
            
            public bool IsPublished { get; set; } = false;
        }

        public class UpdateQuizDto
        {
            public string? Title { get; set; }
            public DateTime? DueAt { get; set; }
            public int? TimeLimitMinutes { get; set; }
            public bool? IsPublished { get; set; }
        }

        public class QuizDto
        {
            public long QuizId { get; set; }
            public long CourseId { get; set; }
            public string Title { get; set; } = string.Empty;
            public DateTime? DueAt { get; set; }
            public int? TimeLimitMinutes { get; set; }
            public bool IsPublished { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string? CourseName { get; set; }
            public int QuestionsCount { get; set; }
            public int AttemptsCount { get; set; }
        }
    }
}
