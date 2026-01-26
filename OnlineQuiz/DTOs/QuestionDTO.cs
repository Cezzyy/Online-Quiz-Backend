using System.ComponentModel.DataAnnotations;

namespace OnlineQuiz.DTOs
{
    public class QuestionDTO
    {
        public class CreateChoiceDto
        {
            [Required]
            public string Body { get; set; } = string.Empty;

            public bool IsCorrect { get; set; } = false;
        }

        public class CreateQuestionDto
        {
            [Required]
            public long QuizId { get; set; }

            [Required]
            [StringLength(16)]
            public string Type { get; set; } = "Single"; // Single, Multiple, Text

            [Required]
            public string Body { get; set; } = string.Empty;

            public decimal Points { get; set; } = 1;

            public List<CreateChoiceDto> Choices { get; set; } = new List<CreateChoiceDto>();
        }

        public class UpdateQuestionDto
        {
            public string? Type { get; set; }
            public string? Body { get; set; }
            public decimal? Points { get; set; }
            public int? SortOrder { get; set; }
            public List<UpdateChoiceDto>? Choices { get; set; }
        }

        public class UpdateChoiceDto
        {
            public long? ChoiceId { get; set; } // If null, it's a new choice
            public string? Body { get; set; }
            public bool? IsCorrect { get; set; }
            public bool IsDeleted { get; set; } = false; // To mark for deletion
        }

        public class ChoiceDto
        {
            public long ChoiceId { get; set; }
            public string Body { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
        }

        public class QuestionDto
        {
            public long QuestionId { get; set; }
            public long QuizId { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
            public decimal Points { get; set; }
            public int SortOrder { get; set; }
            public List<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
        }

        public class StudentChoiceDto
        {
            public long ChoiceId { get; set; }
            public string Body { get; set; } = string.Empty;
        }

        public class StudentQuestionDto
        {
            public long QuestionId { get; set; }
            public long QuizId { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
            public decimal Points { get; set; }
            public int SortOrder { get; set; }
            public List<StudentChoiceDto> Choices { get; set; } = new List<StudentChoiceDto>();
        }
    }
}
