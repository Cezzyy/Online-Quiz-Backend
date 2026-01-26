using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IRepository
{
    public interface IQuestionRepository
    {
        Task<ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>> GetQuestionsByQuizIdAsync(long quizId);
        Task<ServiceResponse<QuestionDTO.QuestionDto>> GetQuestionByIdAsync(long id);
        Task<ServiceResponse<QuestionDTO.QuestionDto>> CreateQuestionAsync(QuestionDTO.CreateQuestionDto dto); // Returns created question
        Task<ServiceResponse<QuestionDTO.QuestionDto>> UpdateQuestionAsync(long id, QuestionDTO.UpdateQuestionDto dto);
        Task<ServiceResponse<bool>> DeleteQuestionAsync(long id);
        Task<int> GetNextSortOrderAsync(long quizId);
    }
}
