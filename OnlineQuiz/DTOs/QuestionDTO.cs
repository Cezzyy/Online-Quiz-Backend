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
            public decimal Points { get; set; }
            public int OrderIndex { get; set; }
            
            // Question Metadata
            public string? Explanation { get; set; }
            
            // Media Support
            public string? ImageUrl { get; set; }
            
            public bool IsRequired { get; set; }
            
            // Timestamps
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            
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
            
            [Range(0.01, 100)]
            public decimal Points { get; set; } = 1;
            
            public int OrderIndex { get; set; }
            
            // Question Metadata
            [StringLength(1000)]
            public string? Explanation { get; set; }
            
            // Media Support
            [StringLength(500)]
            public string? ImageUrl { get; set; }
            
            public bool IsRequired { get; set; } = true;
            
            public List<CreateChoiceDto> Choices { get; set; } = new();
        }

        public class UpdateQuestionDto
        {
            [StringLength(1000)]
            public string? QuestionText { get; set; }
            
            [StringLength(50)]
            public string? QuestionType { get; set; }
            
            [Range(0.01, 100)]
            public decimal? Points { get; set; }
            
            public int? OrderIndex { get; set; }
            
            // Question Metadata
            [StringLength(1000)]
            public string? Explanation { get; set; }
            
            // Media Support
            [StringLength(500)]
            public string? ImageUrl { get; set; }
            
            public bool? IsRequired { get; set; }
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
