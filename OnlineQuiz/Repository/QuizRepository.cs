using Microsoft.EntityFrameworkCore;
using AutoMapper;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Repository
{
    public class QuizRepository : IQuizRepository
    {
        private readonly OnlineQuizDbContext _context;
        private readonly IMapper _mapper;

        public QuizRepository(OnlineQuizDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetAllQuizzesAsync()
        {
            var response = new ServiceResponse<IEnumerable<QuizDTO.QuizDto>>();

            try
            {
                var quizzes = await _context.Quizzes
                    .Where(q => !q.IsDeleted)
                    .Include(q => q.Course)
                    .ThenInclude(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(q => q.Questions)
                    .Include(q => q.Attempts)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<QuizDTO.QuizDto>>(quizzes);
                response.Message = "Quizzes retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving quizzes: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<QuizDTO.QuizDto>> GetQuizByIdAsync(long id)
        {
            var response = new ServiceResponse<QuizDTO.QuizDto>();

            var quiz = await _context.Quizzes
                .Where(q => !q.IsDeleted)
                .Include(q => q.Course)
                .ThenInclude(c => c.Instructor)
                .ThenInclude(t => t.User)
                .Include(q => q.Questions)
                .Include(q => q.Attempts)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                response.Success = false;
                response.Message = "Quiz not found.";
                return response;
            }

            response.Data = _mapper.Map<QuizDTO.QuizDto>(quiz);
            return response;
        }

        public async Task<ServiceResponse<QuizDTO.QuizDto>> CreateQuizAsync(QuizDTO.CreateQuizDto dto, long createdByUserId)
        {
            var response = new ServiceResponse<QuizDTO.QuizDto>();

            try
            {
                // Verify course exists and user has access
                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .FirstOrDefaultAsync(c => c.CourseId == dto.CourseId);

                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                var model = _mapper.Map<QuizModel>(dto);
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                model.IsDeleted = false;

                _context.Quizzes.Add(model);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var createdQuiz = await _context.Quizzes
                    .Where(q => !q.IsDeleted)
                    .Include(q => q.Course)
                    .ThenInclude(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(q => q.Questions)
                    .Include(q => q.Attempts)
                    .FirstOrDefaultAsync(q => q.QuizId == model.QuizId);

                response.Data = _mapper.Map<QuizDTO.QuizDto>(createdQuiz);
                response.Message = "Quiz created successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating quiz: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<(QuizDTO.QuizDto UpdatedQuiz, object OldValues)>> UpdateQuizAsync(long id, QuizDTO.UpdateQuizDto dto)
        {
            var response = new ServiceResponse<(QuizDTO.QuizDto UpdatedQuiz, object OldValues)>();
            
            try
            {
                var quiz = await _context.Quizzes
                    .Where(q => !q.IsDeleted)
                    .FirstOrDefaultAsync(q => q.QuizId == id);

                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Quiz not found.";
                    return response;
                }

                // Capture old values for logging
                var oldValues = new
                {
                    quiz.Title,
                    quiz.Description,
                    quiz.Instructions,
                    quiz.DueAt,
                    quiz.TimeLimitMinutes,
                    quiz.MaxAttempts,
                    quiz.ShuffleQuestions,
                    quiz.ShuffleChoices,
                    quiz.AvailableFrom,
                    quiz.AvailableUntil,
                    quiz.PassingScore,
                    quiz.ShowCorrectAnswers,
                    quiz.ShowScoreImmediately,
                    quiz.IsPublished
                };

                if (!string.IsNullOrWhiteSpace(dto.Title)) quiz.Title = dto.Title;
                if (dto.Description != null) quiz.Description = dto.Description;
                if (dto.Instructions != null) quiz.Instructions = dto.Instructions;
                if (dto.DueAt.HasValue) quiz.DueAt = dto.DueAt.Value;
                if (dto.TimeLimitMinutes.HasValue) quiz.TimeLimitMinutes = dto.TimeLimitMinutes.Value;
                if (dto.MaxAttempts.HasValue) quiz.MaxAttempts = dto.MaxAttempts.Value;
                if (dto.ShuffleQuestions.HasValue) quiz.ShuffleQuestions = dto.ShuffleQuestions.Value;
                if (dto.ShuffleChoices.HasValue) quiz.ShuffleChoices = dto.ShuffleChoices.Value;
                if (dto.AvailableFrom.HasValue) quiz.AvailableFrom = dto.AvailableFrom.Value;
                if (dto.AvailableUntil.HasValue) quiz.AvailableUntil = dto.AvailableUntil.Value;
                if (dto.PassingScore.HasValue) quiz.PassingScore = dto.PassingScore.Value;
                if (dto.ShowCorrectAnswers.HasValue) quiz.ShowCorrectAnswers = dto.ShowCorrectAnswers.Value;
                if (dto.ShowScoreImmediately.HasValue) quiz.ShowScoreImmediately = dto.ShowScoreImmediately.Value;
                if (dto.IsPublished.HasValue) quiz.IsPublished = dto.IsPublished.Value;
                
                quiz.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var updatedQuiz = await _context.Quizzes
                    .Where(q => !q.IsDeleted)
                    .Include(q => q.Course)
                    .ThenInclude(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(q => q.Questions)
                    .Include(q => q.Attempts)
                    .FirstOrDefaultAsync(q => q.QuizId == id);

                var quizDto = _mapper.Map<QuizDTO.QuizDto>(updatedQuiz);
                response.Data = (quizDto, oldValues);
                response.Message = "Quiz updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating quiz: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<(bool Deleted, object QuizInfo)>> DeleteQuizAsync(long id)
        {
            var response = new ServiceResponse<(bool Deleted, object QuizInfo)>();
            
            try
            {
                var quiz = await _context.Quizzes
                    .Where(q => !q.IsDeleted)
                    .FirstOrDefaultAsync(q => q.QuizId == id);

                if (quiz == null)
                {
                    response.Success = false;
                    response.Message = "Quiz not found.";
                    return response;
                }

                // Capture quiz info for logging before soft deletion
                var quizInfo = new
                {
                    quiz.QuizId,
                    quiz.CourseId,
                    quiz.Title,
                    quiz.Description,
                    quiz.DueAt,
                    quiz.TimeLimitMinutes,
                    quiz.IsPublished,
                    quiz.CreatedAt,
                    quiz.UpdatedAt
                };

                // Soft delete
                quiz.IsDeleted = true;
                await _context.SaveChangesAsync();

                response.Data = (true, quizInfo);
                response.Message = "Quiz deleted successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting quiz: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetQuizzesByCourseAsync(long courseId)
        {
            var response = new ServiceResponse<IEnumerable<QuizDTO.QuizDto>>();

            try
            {
                var quizzes = await _context.Quizzes
                    .Where(q => !q.IsDeleted)
                    .Include(q => q.Course)
                    .ThenInclude(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(q => q.Questions)
                    .Include(q => q.Attempts)
                    .Where(q => q.CourseId == courseId)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<QuizDTO.QuizDto>>(quizzes);
                response.Message = "Course quizzes retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving course quizzes: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<QuizDTO.QuizDto>>> GetQuizzesByInstructorAsync(long instructorId)
        {
            var response = new ServiceResponse<IEnumerable<QuizDTO.QuizDto>>();

            try
            {
                var quizzes = await _context.Quizzes
                    .Where(q => !q.IsDeleted)
                    .Include(q => q.Course)
                    .ThenInclude(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(q => q.Questions)
                    .Include(q => q.Attempts)
                    .Where(q => q.Course.InstructorUserId == instructorId)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<QuizDTO.QuizDto>>(quizzes);
                response.Message = "Instructor quizzes retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving instructor quizzes: {ex.Message}";
            }

            return response;
        }
    }
}
