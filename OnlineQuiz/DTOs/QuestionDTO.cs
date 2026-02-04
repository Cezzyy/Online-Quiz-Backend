using System.ComponentModel.DataAnnotations;

namespace OnlineQuiz.DTOs
{
    public class QuestionDTO
    {
        public class QuestionDto
        {
            public long QuestionId { get; set; }
            public long QuizId { get; set; }
            public string QuestionText { get; set; } = string.Empty;
            public string QuestionType { get; set; } = string.Empty;
            public int Points { get; set; }
            public int OrderIndex { get; set; }
            public List<ChoiceDto> Choices { get; set; } = new();
        }

        public class CreateQuestionDto
        {
            [Required]
            [StringLength(1000)]
            public string QuestionText { get; set; } = string.Empty;
            
            [Required]
            [StringLength(50)]
            public string QuestionType { get; set; } = "MultipleChoice";
            
            [Range(1, 100)]
            public int Points { get; set; } = 1;
            
            public int OrderIndex { get; set; }
            
            public List<CreateChoiceDto> Choices { get; set; } = new();
        }

        public class ChoiceDto
        {
            public long ChoiceId { get; set; }
            public long QuestionId { get; set; }
            public string ChoiceText { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
            public int OrderIndex { get; set; }
        }

        public class CreateChoiceDto
        {
            [Required]
            [StringLength(500)]
            public string ChoiceText { get; set; } = string.Empty;
            
            public bool IsCorrect { get; set; }
            
            public int OrderIndex { get; set; }
        }
    }
}
