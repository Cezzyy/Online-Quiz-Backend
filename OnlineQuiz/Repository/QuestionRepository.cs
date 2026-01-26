using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly OnlineQuizDbContext _context;
        private readonly IMapper _mapper;

        public QuestionRepository(OnlineQuizDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>> GetQuestionsByQuizIdAsync(long quizId)
        {
            var response = new ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>();
            try
            {
                var questions = await _context.Questions
                    .Where(q => q.QuizId == quizId)
                    .Include(q => q.Choices)
                    .OrderBy(q => q.SortOrder)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<QuestionDTO.QuestionDto>>(questions);
                response.Success = true;
                response.Message = "Questions retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving questions: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<QuestionDTO.QuestionDto>> GetQuestionByIdAsync(long id)
        {
            var response = new ServiceResponse<QuestionDTO.QuestionDto>();
            try
            {
                var question = await _context.Questions
                    .Include(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.QuestionId == id);

                if (question == null)
                {
                    response.Success = false;
                    response.Message = "Question not found.";
                    return response;
                }

                response.Data = _mapper.Map<QuestionDTO.QuestionDto>(question);
                response.Success = true;
                response.Message = "Question retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving question: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<QuestionDTO.QuestionDto>> CreateQuestionAsync(QuestionDTO.CreateQuestionDto dto)
        {
            var response = new ServiceResponse<QuestionDTO.QuestionDto>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var question = _mapper.Map<QuestionModel>(dto);
                
                // Auto-increment sort order
                var maxSortOrder = await _context.Questions
                    .Where(q => q.QuizId == dto.QuizId)
                    .MaxAsync(q => (int?)q.SortOrder) ?? 0;
                question.SortOrder = maxSortOrder + 1;

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                // Add Choices if any
                if (dto.Choices != null && dto.Choices.Any())
                {
                    foreach (var choiceDto in dto.Choices)
                    {
                        var choice = _mapper.Map<ChoiceModel>(choiceDto);
                        choice.QuestionId = question.QuestionId;
                        _context.Choices.Add(choice);
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Reload to get everything including IDs
                var createdQuestion = await _context.Questions
                    .Include(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.QuestionId == question.QuestionId);

                response.Data = _mapper.Map<QuestionDTO.QuestionDto>(createdQuestion);
                response.Success = true;
                response.Message = "Question created successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Error creating question: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<QuestionDTO.QuestionDto>> UpdateQuestionAsync(long id, QuestionDTO.UpdateQuestionDto dto)
        {
            var response = new ServiceResponse<QuestionDTO.QuestionDto>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var question = await _context.Questions
                    .Include(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.QuestionId == id);

                if (question == null)
                {
                    response.Success = false;
                    response.Message = "Question not found.";
                    return response;
                }

                // Update properties if provided
                if (!string.IsNullOrEmpty(dto.Type)) question.Type = dto.Type;
                if (!string.IsNullOrEmpty(dto.Body)) question.Body = dto.Body;
                if (dto.Points.HasValue) question.Points = dto.Points.Value;
                if (dto.SortOrder.HasValue) question.SortOrder = dto.SortOrder.Value;

                await _context.SaveChangesAsync();

                // Handle Choices
                if (dto.Choices != null)
                {
                    foreach (var choiceDto in dto.Choices)
                    {
                        if (choiceDto.IsDeleted)
                        {
                            if (choiceDto.ChoiceId.HasValue)
                            {
                                var choiceToDelete = question.Choices.FirstOrDefault(c => c.ChoiceId == choiceDto.ChoiceId.Value);
                                if (choiceToDelete != null)
                                    _context.Choices.Remove(choiceToDelete);
                            }
                        }
                        else if (choiceDto.ChoiceId.HasValue)
                        {
                            // Update existing choice
                            var existingChoice = question.Choices.FirstOrDefault(c => c.ChoiceId == choiceDto.ChoiceId.Value);
                            if (existingChoice != null)
                            {
                                if (!string.IsNullOrEmpty(choiceDto.Body)) existingChoice.Body = choiceDto.Body;
                                if (choiceDto.IsCorrect.HasValue) existingChoice.IsCorrect = choiceDto.IsCorrect.Value;
                            }
                        }
                        else
                        {
                            // Create new choice
                            var newChoice = new ChoiceModel
                            {
                                QuestionId = question.QuestionId,
                                Body = choiceDto.Body ?? string.Empty,
                                IsCorrect = choiceDto.IsCorrect ?? false
                            };
                            _context.Choices.Add(newChoice);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                response.Data = _mapper.Map<QuestionDTO.QuestionDto>(question);
                response.Success = true;
                response.Message = "Question updated successfully.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = $"Error updating question: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteQuestionAsync(long id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var question = await _context.Questions.FindAsync(id);
                if (question == null)
                {
                    response.Success = false;
                    response.Message = "Question not found.";
                    response.Data = false;
                    return response;
                }

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Success = true;
                response.Message = "Question deleted successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting question: {ex.Message}";
                response.Data = false;
            }
            return response;
        }

        public async Task<int> GetNextSortOrderAsync(long quizId)
        {
            var maxSortOrder = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .MaxAsync(q => (int?)q.SortOrder);
                
            return (maxSortOrder ?? 0) + 1;
        }
    }
}
