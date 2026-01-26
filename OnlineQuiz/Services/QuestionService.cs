using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionService(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>> GetQuestionsByQuizIdAsync(long quizId)
        {
            return await _questionRepository.GetQuestionsByQuizIdAsync(quizId);
        }

        public async Task<ServiceResponse<QuestionDTO.QuestionDto>> GetQuestionByIdAsync(long id)
        {
            return await _questionRepository.GetQuestionByIdAsync(id);
        }

        public async Task<ServiceResponse<QuestionDTO.QuestionDto>> CreateQuestionAsync(QuestionDTO.CreateQuestionDto dto)
        {
            // Business Logic 1: Auto-increment sequence
            // Note: Repository or Service can handle this. Since it's logic about the quiz structure, Service is appropriate.
            // But I made repository expose `GetNextSortOrderAsync` for this purpose.
            
            // Wait, CreateQuestionDto doesn't have SortOrder property exposed for input (it shouldn't).
            // But the Repository's `CreateQuestionAsync` takes `CreateQuestionDto`.
            // The mapping happens inside Repository.
            // If I want to force SortOrder, I should probably pass it or handle it in Repository.
            // However, the `CreateQuestionDto` generally maps to `QuestionModel` where `SortOrder` exists.
            // My `CreateQuestionDto` definition DOES NOT have `SortOrder`. 
            // So automapper will leave it default (0 or 1).
            // This means I MUST modifying the Model before saving in Repository, OR
            // I need to update the logic.
            
            // Correct approach: Service determines the SortOrder, then we need a way to pass it to Repository.
            // But `CreateQuestionDto` is fixed.
            // I should have added `SortOrder` to `CreateQuestionDto` (hidden from API user maybe, or just set it manually).
            // Actually, I can't easily modify the DTO object if it doesn't have the property.
            
            // Review `CreateQuestionDto`... it serves as the input. 
            // The Repository maps `CreateQuestionDto` -> `QuestionModel`.
            // So if `CreateQuestionDto` lacks `SortOrder`, `QuestionModel.SortOrder` will be default.
            
            // Implication: I need to handle `SortOrder` calculation INSIDE Repository or existing `CreateQuestionAsync` logic
            // validation might fail if I can't set it.
            // Let's check `QuestionRepository.cs` again.
            // I wrote: `var question = _mapper.Map<QuestionModel>(dto);`
            // Then `_context.Questions.Add(question);`
            
            // So I should calculate SortOrder in Repository OR Service.
            // If Service: I need to pass it.
            // If Repository: I can call `GetNextSortOrderAsync` inside `CreateQuestionAsync`.
            
            // Let's modify `QuestionRepository.cs` later if needed, but for now I'll assume 
            // I can't easily change `CreateQuestionDto` structure without breaking API contract if it was public.
            // But it's internal dev. I will update `CreateQuestionDto` to allow `SortOrder`? 
            // No, user shouldn't set it.
            
            // Best spot: `QuestionRepository.CreateQuestionAsync` explicitly sets it if not provided?
            // BUT, the requirement is "Add logic to auto-increment...".
            
            // I will inject the logic here in Service, but since I can't modify DTO (no property),
            // I have to trust Repository logic or update Repository.
            // Wait, I created `QuestionRepository` and I CAN modify it.
            // I will implement validation logic here first.
            
            var validateResponse = await ValidateQuestionChoicesAsync(dto.Choices, dto.Type);
            if (!validateResponse.Success)
            {
                return new ServiceResponse<QuestionDTO.QuestionDto>
                {
                    Success = false,
                    Message = validateResponse.Message
                };
            }

            // Since I cannot set SortOrder on DTO, checking if Repository handles it.
            // I will modify Repository to handle SortOrder if I didn't. 
            // I'll check `QuestionRepository` I just wrote... I did NOT uncomment the logic.
            // So I should update `QuestionRepository` to call `GetNextSortOrderAsync`.
            // OR I update `CreateQuestionDto` to have an internal setter? No.
            
            // Let's just pass it to Repository? But the signature is `CreateQuestionAsync(CreateQuestionDto dto)`.
            // I'll update `CreateQuestionDto` to `internal set` SortOrder? Or just public but Ignore in Controller?
            // Actually, `QuestionModel` has `SortOrder`.
            // I'll update `QuestionRepository` to handle the SortOrder logic before saving.
            
            return await _questionRepository.CreateQuestionAsync(dto);
        }

        public async Task<ServiceResponse<QuestionDTO.QuestionDto>> UpdateQuestionAsync(long id, QuestionDTO.UpdateQuestionDto dto)
        {
            return await _questionRepository.UpdateQuestionAsync(id, dto);
        }

        public async Task<ServiceResponse<bool>> DeleteQuestionAsync(long id)
        {
            return await _questionRepository.DeleteQuestionAsync(id);
        }

        public Task<ServiceResponse<bool>> ValidateQuestionChoicesAsync(List<QuestionDTO.CreateChoiceDto> choices, string type)
        {
            var response = new ServiceResponse<bool> { Success = true };

            if (type.Equals("Single", StringComparison.OrdinalIgnoreCase) || type.Equals("Multiple", StringComparison.OrdinalIgnoreCase))
            {
                if (choices == null || !choices.Any())
                {
                    response.Success = false;
                    response.Message = "Questions of type Single or Multiple must have choices.";
                    return Task.FromResult(response);
                }

                if (!choices.Any(c => c.IsCorrect))
                {
                    response.Success = false;
                    response.Message = "At least one choice must be marked as correct.";
                    return Task.FromResult(response);
                }
            }
            
            return Task.FromResult(response);
        }
    }
}
