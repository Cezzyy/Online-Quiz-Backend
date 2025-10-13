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
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllCoursesAsync());

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById([FromRoute] long id) =>
            Ok(await _service.GetCourseByIdAsync(id));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseDTO.CreateCourseDto dto)
        {
            var currentUserId = GetCurrentUserId();
            return Ok(await _service.CreateCourseAsync(dto, currentUserId));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] CourseDTO.UpdateCourseDto dto) =>
            Ok(await _service.UpdateCourseAsync(id, dto));

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id) =>
            Ok(await _service.DeleteCourseAsync(id));

        [Authorize(Roles = "Teacher")]
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = GetCurrentUserId();
            return Ok(await _service.GetCoursesByInstructorAsync(userId));
        }

        [Authorize(Roles = "Student")]
        [HttpGet("my-enrolled-courses")]
        public async Task<IActionResult> GetMyEnrolledCourses()
        {
            var userId = GetCurrentUserId();
            return Ok(await _service.GetEnrolledCoursesByStudentAsync(userId));
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("{courseId}/students")]
        public async Task<IActionResult> GetCourseStudents([FromRoute] long courseId) =>
            Ok(await _service.GetCourseStudentsAsync(courseId));

        [Authorize(Roles = "Teacher")]
        [HttpPost("{courseId}/enroll-student")]
        public async Task<IActionResult> EnrollStudent([FromRoute] long courseId, [FromBody] EnrollStudentDto dto) =>
            Ok(await _service.EnrollStudentInCourseAsync(courseId, dto.StudentId));

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{courseId}/unenroll-student/{studentId}")]
        public async Task<IActionResult> UnenrollStudent([FromRoute] long courseId, [FromRoute] long studentId) =>
            Ok(await _service.UnenrollStudentFromCourseAsync(courseId, studentId));

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

    public class EnrollStudentDto
    {
        public long StudentId { get; set; }
    }
}
