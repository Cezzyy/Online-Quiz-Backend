using System.ComponentModel.DataAnnotations;

namespace OnlineQuiz.DTOs
{
    public class AttemptDTO
    {
        public class AttemptDto
        {
            public long AttemptId { get; set; }
            public long QuizId { get; set; }
            public long UserId { get; set; }
            public string? UserName { get; set; }
            public string? UserEmail { get; set; }
            public DateTime StartedAt { get; set; }
            public DateTime? SubmittedAt { get; set; }
            public decimal Score { get; set; }
            public int? TimeSpentSeconds { get; set; }
            public string Status { get; set; } = "InProgress";
            public decimal? Percentage { get; set; }
            public bool IsPassed { get; set; }
            public string? InstructorFeedback { get; set; }
            public long? GradedBy { get; set; }
            public DateTime? GradedAt { get; set; }
            public int TabSwitchCount { get; set; }
            public bool FlaggedForReview { get; set; }
            public string? ReviewNotes { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class CreateAttemptDto
        {
            [Required]
            public long QuizId { get; set; }
        }

        public class SubmitAttemptDto
        {
            [Required]
            public long AttemptId { get; set; }
            
            public List<SubmitAnswerDto> Answers { get; set; } = new();
        }

        public class SubmitAnswerDto
        {
            [Required]
            public long QuestionId { get; set; }
            
            public long? ChoiceId { get; set; }
            
            public string? FreeText { get; set; }
            
            public int TimeSpentSeconds { get; set; }
        }

        public class UpdateAttemptDto
        {
            [StringLength(2000)]
            public string? InstructorFeedback { get; set; }
            
            public bool? FlaggedForReview { get; set; }
            
            [StringLength(1000)]
            public string? ReviewNotes { get; set; }
        }

        public class GradeAttemptDto
        {
            [Required]
            public decimal Score { get; set; }
            
            [StringLength(2000)]
            public string? InstructorFeedback { get; set; }
        }

        public class AttemptAnswerDto
        {
            public long AttemptAnswerId { get; set; }
            public long AttemptId { get; set; }
            public long QuestionId { get; set; }
            public string? QuestionText { get; set; }
            public long? ChoiceId { get; set; }
            public string? ChoiceText { get; set; }
            public string? FreeText { get; set; }
            public bool? IsCorrect { get; set; }
            public DateTime AnsweredAt { get; set; }
            public int TimeSpentSeconds { get; set; }
            public decimal? PointsEarned { get; set; }
            public decimal? PointsPossible { get; set; }
            public string? Feedback { get; set; }
            public bool IsSkipped { get; set; }
        }
    }
}
