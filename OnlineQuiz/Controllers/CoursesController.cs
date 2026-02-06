using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.DTOs;
using OnlineQuiz.Services;
using OnlineQuiz.IServices;
using static OnlineQuiz.DTOs.CourseDTO;
using static OnlineQuiz.DTOs.EnrollmentDTO;

namespace OnlineQuiz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<CourseDTO.PagedCoursesDto>> GetCoursesWithFilter([FromQuery] CourseDTO.CourseFilterDto filter)
        {
            var result = await _courseService.GetCoursesWithFilterAsync(filter);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourse(long id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpGet("{id}/statistics")]
        public async Task<ActionResult<CourseDTO.CourseStatisticsDto>> GetCourseStatistics(long id)
        {
            var result = await _courseService.GetCourseStatisticsAsync(id);
            if (!result.Success)
            {
                return NotFound(result.Message);
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CourseDto>> CreateCourse(CreateCourseDto createCourseDto)
        {
            try
            {
                var course = await _courseService.CreateCourseAsync(createCourseDto);
                if (!course.Success)
                    return BadRequest(course.Message);
                
                if (course.Data == null)
                    return BadRequest("Failed to create course");
                
                return CreatedAtAction(nameof(GetCourse), new { id = course.Data.CourseId }, course.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CourseDto>> UpdateCourse(long id, UpdateCourseDto updateCourseDto)
        {
            var course = await _courseService.UpdateCourseAsync(id, updateCourseDto);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(long id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("instructor/{instructorId}")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesByInstructor(long instructorId)
        {
            var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);
            return Ok(courses);
        }

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesByStudent(long studentId)
        {
            var courses = await _courseService.GetCoursesByStudentAsync(studentId);
            return Ok(courses);
        }

        [HttpPost("{id}/enrollments")]
        public async Task<ActionResult<EnrollmentDto>> EnrollStudent(long id, [FromBody] EnrollStudentDto enrollStudentDto)
        {
            try
            {
                var createEnrollmentDto = new CreateEnrollmentDto
                {
                    UserId = enrollStudentDto.UserId,
                    CourseId = id
                };
                var enrollment = await _courseService.EnrollStudentAsync(createEnrollmentDto);
                return Ok(enrollment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}/enrollments/{userId}")]
        public async Task<IActionResult> UnenrollStudent(long id, long userId)
        {
            var result = await _courseService.UnenrollStudentAsync(userId, id);
            if (!result.Success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("{id}/enrollments")]
        public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollments(long id)
        {
            var enrollments = await _courseService.GetCourseEnrollmentsAsync(id);
            return Ok(enrollments);
        }

        [HttpGet("{id}/enrollments/{userId}/check")]
        public async Task<ActionResult<bool>> CheckEnrollment(long id, long userId)
        {
            var isEnrolled = await _courseService.IsStudentEnrolledAsync(userId, id);
            return Ok(isEnrolled);
        }
    }
}