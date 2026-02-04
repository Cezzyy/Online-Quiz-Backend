using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IServices
{
    public interface IQuizService
    {
        Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetAllQuizzesAsync();
        Task<ServiceResponse<QuizDTO.QuizDto>> GetQuizByIdAsync(long id);
        Task<ServiceResponse<QuizDTO.QuizDto>> CreateQuizAsync(QuizDTO.CreateQuizDto dto);
        Task<ServiceResponse<QuizDTO.QuizDto>> UpdateQuizAsync(long id, QuizDTO.UpdateQuizDto dto);
        Task<ServiceResponse<bool>> DeleteQuizAsync(long id);
        
        // Course-specific quiz methods
        Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetQuizzesByCourseAsync(long courseId);
        Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetPublishedQuizzesByCourseAsync(long courseId);
        
        // Quiz publishing
        Task<ServiceResponse<bool>> PublishQuizAsync(long id);
        Task<ServiceResponse<bool>> UnpublishQuizAsync(long id);
        
        // Question management
        Task<ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>> GetQuestionsByQuizAsync(long quizId);
        Task<ServiceResponse<QuestionDTO.QuestionDto>> AddQuestionAsync(long quizId, QuestionDTO.CreateQuestionDto dto);
        Task<ServiceResponse<QuestionDTO.QuestionDto>> UpdateQuestionAsync(long questionId, QuestionDTO.CreateQuestionDto dto);
        Task<ServiceResponse<bool>> DeleteQuestionAsync(long questionId);
    }
}
