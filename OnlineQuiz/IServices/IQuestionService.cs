using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IServices
{
    public interface IQuestionService
    {
        Task<ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>> GetQuestionsByQuizIdAsync(long quizId);
        Task<ServiceResponse<QuestionDTO.QuestionDto>> GetQuestionByIdAsync(long id);
        Task<ServiceResponse<QuestionDTO.QuestionDto>> CreateQuestionAsync(QuestionDTO.CreateQuestionDto dto);
        Task<ServiceResponse<QuestionDTO.QuestionDto>> UpdateQuestionAsync(long id, QuestionDTO.UpdateQuestionDto dto);
        Task<ServiceResponse<bool>> DeleteQuestionAsync(long id);
        Task<ServiceResponse<bool>> ValidateQuestionChoicesAsync(List<QuestionDTO.CreateChoiceDto> choices, string type);
    }
}
