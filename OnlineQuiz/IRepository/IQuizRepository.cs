using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IRepository
{
    public interface IQuizRepository
    {
        Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetAllQuizzesAsync();
        Task<ServiceResponse<QuizDTO.QuizDto>> GetQuizByIdAsync(long id);
        Task<ServiceResponse<QuizDTO.QuizDto>> CreateQuizAsync(QuizDTO.CreateQuizDto dto, long createdByUserId);
        Task<ServiceResponse<(QuizDTO.QuizDto UpdatedQuiz, object OldValues)>> UpdateQuizAsync(long id, QuizDTO.UpdateQuizDto dto);
        Task<ServiceResponse<(bool Deleted, object QuizInfo)>> DeleteQuizAsync(long id);
        
        // Course-specific quiz methods
        Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetQuizzesByCourseAsync(long courseId);
        Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetQuizzesByInstructorAsync(long instructorId);
    }
}
