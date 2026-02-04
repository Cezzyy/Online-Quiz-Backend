using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.DTOs;
using OnlineQuiz.Services;
using OnlineQuiz.IServices;
using static OnlineQuiz.DTOs.QuizDTO;
using static OnlineQuiz.DTOs.QuestionDTO;

namespace OnlineQuiz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<QuizDto>> GetQuiz(long id)
        {
            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }
            return Ok(quiz);
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetQuizzesByCourse(long courseId)
        {
            var quizzes = await _quizService.GetQuizzesByCourseAsync(courseId);
            return Ok(quizzes);
        }

        [HttpGet("course/{courseId}/published")]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetPublishedQuizzesByCourse(long courseId)
        {
            var quizzes = await _quizService.GetPublishedQuizzesByCourseAsync(courseId);
            return Ok(quizzes);
        }

        [HttpPost]
        public async Task<ActionResult<QuizDto>> CreateQuiz(CreateQuizDto createQuizDto)
        {
            try
            {
                var quiz = await _quizService.CreateQuizAsync(createQuizDto);
                if (!quiz.Success)
                    return BadRequest(quiz.Message);
                
                if (quiz.Data == null)
                    return BadRequest("Failed to create quiz");
                
                return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Data.QuizId }, quiz.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<QuizDto>> UpdateQuiz(long id, UpdateQuizDto updateQuizDto)
        {
            var quiz = await _quizService.UpdateQuizAsync(id, updateQuizDto);
            if (quiz == null)
            {
                return NotFound();
            }
            return Ok(quiz);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(long id)
        {
            var result = await _quizService.DeleteQuizAsync(id);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }
            return NoContent();
        }

        [HttpPost("{id}/publish")]
        public async Task<IActionResult> PublishQuiz(long id)
        {
            var result = await _quizService.PublishQuizAsync(id);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }
            return Ok("Quiz published successfully");
        }

        [HttpPost("{id}/unpublish")]
        public async Task<IActionResult> UnpublishQuiz(long id)
        {
            var result = await _quizService.UnpublishQuizAsync(id);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }
            return Ok("Quiz unpublished successfully");
        }

        [HttpGet("{id}/questions")]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetQuestions(long id)
        {
            var questions = await _quizService.GetQuestionsByQuizAsync(id);
            return Ok(questions);
        }

        [HttpPost("{quizId}/questions")]
        public async Task<ActionResult<QuestionDto>> CreateQuestion(long quizId, CreateQuestionDto createQuestionDto)
        {
            try
            {
                var question = await _quizService.AddQuestionAsync(quizId, createQuestionDto);
                return Ok(question);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("questions/{questionId}")]
        public async Task<ActionResult<QuestionDto>> UpdateQuestion(long questionId, CreateQuestionDto updateQuestionDto)
        {
            var question = await _quizService.UpdateQuestionAsync(questionId, updateQuestionDto);
            if (question == null)
            {
                return NotFound();
            }
            return Ok(question);
        }

        [HttpDelete("questions/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(long questionId)
        {
            var result = await _quizService.DeleteQuestionAsync(questionId);
            if (!result.Success)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}