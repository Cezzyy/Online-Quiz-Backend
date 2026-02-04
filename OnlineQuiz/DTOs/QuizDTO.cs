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
            
            [StringLength(2000)]
            public string? Description { get; set; }
            
            [StringLength(1000)]
            public string? Instructions { get; set; }
            
            public DateTime? DueAt { get; set; }
            
            public int? TimeLimitMinutes { get; set; }
            
            public int? MaxAttempts { get; set; }
            
            public bool ShuffleQuestions { get; set; } = false;
            
            public bool ShuffleChoices { get; set; } = false;
            
            public DateTime? AvailableFrom { get; set; }
            
            public DateTime? AvailableUntil { get; set; }
            
            [Range(0, 100)]
            public decimal PassingScore { get; set; } = 60.00m;
            
            public bool ShowCorrectAnswers { get; set; } = true;
            
            public bool ShowScoreImmediately { get; set; } = true;
            
            public bool IsPublished { get; set; } = false;
        }

        public class UpdateQuizDto
        {
            [StringLength(200)]
            public string? Title { get; set; }
            
            [StringLength(2000)]
            public string? Description { get; set; }
            
            [StringLength(1000)]
            public string? Instructions { get; set; }
            
            public DateTime? DueAt { get; set; }
            
            public int? TimeLimitMinutes { get; set; }
            
            public int? MaxAttempts { get; set; }
            
            public bool? ShuffleQuestions { get; set; }
            
            public bool? ShuffleChoices { get; set; }
            
            public DateTime? AvailableFrom { get; set; }
            
            public DateTime? AvailableUntil { get; set; }
            
            [Range(0, 100)]
            public decimal? PassingScore { get; set; }
            
            public bool? ShowCorrectAnswers { get; set; }
            
            public bool? ShowScoreImmediately { get; set; }
            
            public bool? IsPublished { get; set; }
        }

        public class QuizDto
        {
            public long QuizId { get; set; }
            public long CourseId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Instructions { get; set; }
            public DateTime? DueAt { get; set; }
            public int? TimeLimitMinutes { get; set; }
            public int? MaxAttempts { get; set; }
            public bool ShuffleQuestions { get; set; }
            public bool ShuffleChoices { get; set; }
            public DateTime? AvailableFrom { get; set; }
            public DateTime? AvailableUntil { get; set; }
            public decimal PassingScore { get; set; }
            public bool ShowCorrectAnswers { get; set; }
            public bool ShowScoreImmediately { get; set; }
            public bool IsPublished { get; set; }
            public bool IsDeleted { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string? CourseName { get; set; }
            public int QuestionsCount { get; set; }
            public int AttemptsCount { get; set; }
        }
    }
}
