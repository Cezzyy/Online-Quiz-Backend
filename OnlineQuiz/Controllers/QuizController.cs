using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OnlineQuiz.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _service;
        private readonly IActivityLogService _activityLogService;

        public QuizController(IQuizService service, IActivityLogService activityLogService)
        {
            _service = service;
            _activityLogService = activityLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllQuizzesAsync());

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.GetQuizByIdAsync(id);
            
            if (result.Success && result.Data != null)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "VIEW", "Quiz", 
                    id, $"Viewed quiz: {result.Data.Title}", null, null);
            }
            
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] QuizDTO.CreateQuizDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.CreateQuizAsync(dto, currentUserId);
            
            if (result.Success && result.Data != null)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "CREATE", "Quiz", 
                    result.Data.QuizId, $"Created quiz: {dto.Title}", null, dto);
            }
            
            return Ok(result);
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] QuizDTO.UpdateQuizDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.UpdateQuizAsync(id, dto);
            
            if (result.Success && result.Data.UpdatedQuiz != null)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "UPDATE", "Quiz", 
                    id, $"Updated quiz: {result.Data.UpdatedQuiz.Title}", 
                    result.Data.OldValues, dto);
            }
            
            return Ok(new { 
                Success = result.Success, 
                Data = result.Data.UpdatedQuiz, 
                Message = result.Message 
            });
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.DeleteQuizAsync(id);
            
            if (result.Success && result.Data.Deleted)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "DELETE", "Quiz", 
                    id, $"Deleted quiz: {((dynamic)result.Data.QuizInfo).Title}", 
                    result.Data.QuizInfo, null);
            }
            
            return Ok(new { 
                Success = result.Success, 
                Data = result.Data.Deleted, 
                Message = result.Message 
            });
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse([FromRoute] long courseId) =>
            Ok(await _service.GetQuizzesByCourseAsync(courseId));

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("my-quizzes")]
        public async Task<IActionResult> GetMyQuizzes()
        {
            var userId = GetCurrentUserId();
            return Ok(await _service.GetQuizzesByInstructorAsync(userId));
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token");
        }
    }
}
