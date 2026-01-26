using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        [HttpGet("quiz/{quizId}")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<QuestionDTO.QuestionDto>>>> GetQuestionsByQuizId(long quizId)
        {
            var response = await _questionService.GetQuestionsByQuizIdAsync(quizId);
            if (!response.Success)
            {
                return BadRequest(response); // Or NotFound depending on logic
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<QuestionDTO.QuestionDto>>> GetQuestionById(long id)
        {
            var response = await _questionService.GetQuestionByIdAsync(id);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponse<QuestionDTO.QuestionDto>>> CreateQuestion(QuestionDTO.CreateQuestionDto dto)
        {
            var response = await _questionService.CreateQuestionAsync(dto);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return CreatedAtAction(nameof(GetQuestionById), new { id = response.Data?.QuestionId }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponse<QuestionDTO.QuestionDto>>> UpdateQuestion(long id, QuestionDTO.UpdateQuestionDto dto)
        {
            var response = await _questionService.UpdateQuestionAsync(id, dto);
            if (!response.Success)
            {
                if (response.Message == "Question not found.")
                    return NotFound(response);
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteQuestion(long id)
        {
            var response = await _questionService.DeleteQuestionAsync(id);
            if (!response.Success)
            {
                if (response.Message == "Question not found.")
                    return NotFound(response);
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
