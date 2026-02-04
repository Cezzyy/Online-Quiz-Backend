using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace OnlineQuiz.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuizService(IQuizRepository quizRepository, IHttpContextAccessor httpContextAccessor)
        {
            _quizRepository = quizRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in authentication token");
        }

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetAllQuizzesAsync() =>
            await _quizRepository.GetAllQuizzesAsync();

        public async Task<ServiceResponse<QuizDTO.QuizDto>> GetQuizByIdAsync(long id) =>
            await _quizRepository.GetQuizByIdAsync(id);

        public async Task<ServiceResponse<QuizDTO.QuizDto>> CreateQuizAsync(QuizDTO.CreateQuizDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                return await _quizRepository.CreateQuizAsync(dto, currentUserId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return new ServiceResponse<QuizDTO.QuizDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<QuizDTO.QuizDto>> UpdateQuizAsync(long id, QuizDTO.UpdateQuizDto dto)
        {
            var result = await _quizRepository.UpdateQuizAsync(id, dto);
            if (!result.Success)
                return new ServiceResponse<QuizDTO.QuizDto>(result.Message ?? "Failed to update quiz");
            return new ServiceResponse<QuizDTO.QuizDto>(result.Data.UpdatedQuiz);
        }

        public async Task<ServiceResponse<bool>> DeleteQuizAsync(long id)
        {
            var result = await _quizRepository.DeleteQuizAsync(id);
            if (!result.Success)
                return new ServiceResponse<bool>(result.Message ?? "Failed to delete quiz");
            return new ServiceResponse<bool>(result.Data.Deleted);
        }

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetQuizzesByCourseAsync(long courseId) =>
            await _quizRepository.GetQuizzesByCourseAsync(courseId);

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetPublishedQuizzesByCourseAsync(long courseId)
        {
            var result = await _quizRepository.GetQuizzesByCourseAsync(courseId);
            if (!result.Success)
                return result;
            
            // Filter published quizzes
            var publishedQuizzes = result.Data?.Where(q => q.IsPublished).ToList();
            return new ServiceResponse<IEnumerable<QuizDTO.QuizDto>>(publishedQuizzes);
        }

        public async Task<ServiceResponse<bool>> PublishQuizAsync(long id)
        {
            var updateDto = new QuizDTO.UpdateQuizDto { IsPublished = true };
            var result = await _quizRepository.UpdateQuizAsync(id, updateDto);
            if (!result.Success)
                return new ServiceResponse<bool>(result.Message ?? "Failed to publish quiz");
            return new ServiceResponse<bool>(true);
        }

        public async Task<ServiceResponse<bool>> UnpublishQuizAsync(long id)
        {
            var updateDto = new QuizDTO.UpdateQuizDto { IsPublished = false };
            var result = await _quizRepository.UpdateQuizAsync(id, updateDto);
            if (!result.Success)
                return new ServiceResponse<bool>(result.Message ?? "Failed to unpublish quiz");
            return new ServiceResponse<bool>(true);
        }

        public Task<ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>> GetQuestionsByQuizAsync(long quizId)
        {
            // This method needs to be implemented in the repository
            // For now, return empty list
            return Task.FromResult(new ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>(new List<QuestionDTO.QuestionDto>()));
        }

        public Task<ServiceResponse<QuestionDTO.QuestionDto>> AddQuestionAsync(long quizId, QuestionDTO.CreateQuestionDto dto)
        {
            // This method needs to be implemented in the repository
            // For now, return error
            return Task.FromResult(new ServiceResponse<QuestionDTO.QuestionDto>("Method not implemented"));
        }

        public Task<ServiceResponse<QuestionDTO.QuestionDto>> UpdateQuestionAsync(long questionId, QuestionDTO.CreateQuestionDto dto)
        {
            // This method needs to be implemented in the repository
            // For now, return error
            return Task.FromResult(new ServiceResponse<QuestionDTO.QuestionDto>("Method not implemented"));
        }

        public Task<ServiceResponse<bool>> DeleteQuestionAsync(long questionId)
        {
            // This method needs to be implemented in the repository
            // For now, return error
            return Task.FromResult(new ServiceResponse<bool>("Method not implemented"));
        }
    }
}
