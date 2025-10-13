using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _repo;

        public QuizService(IQuizRepository repo)
        {
            _repo = repo;
        }

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetAllQuizzesAsync() =>
            await _repo.GetAllQuizzesAsync();

        public async Task<ServiceResponse<QuizDTO.QuizDto>> GetQuizByIdAsync(long id) =>
            await _repo.GetQuizByIdAsync(id);

        public async Task<ServiceResponse<QuizDTO.QuizDto>> CreateQuizAsync(QuizDTO.CreateQuizDto dto, long createdByUserId) =>
            await _repo.CreateQuizAsync(dto, createdByUserId);

        public async Task<ServiceResponse<(QuizDTO.QuizDto UpdatedQuiz, object OldValues)>> UpdateQuizAsync(long id, QuizDTO.UpdateQuizDto dto) =>
            await _repo.UpdateQuizAsync(id, dto);

        public async Task<ServiceResponse<(bool Deleted, object QuizInfo)>> DeleteQuizAsync(long id) =>
            await _repo.DeleteQuizAsync(id);

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetQuizzesByCourseAsync(long courseId) =>
            await _repo.GetQuizzesByCourseAsync(courseId);

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetQuizzesByInstructorAsync(long instructorId) =>
            await _repo.GetQuizzesByInstructorAsync(instructorId);
    }
}
