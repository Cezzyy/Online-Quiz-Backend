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
        private readonly IActivityLogService _activityLogService;

        public CourseController(ICourseService service, IActivityLogService activityLogService)
        {
            _service = service;
            _activityLogService = activityLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllCoursesAsync());

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById([FromRoute] long id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.GetCourseByIdAsync(id);
            
            if (result.Success && result.Data != null)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "VIEW", "Course", 
                    id, $"Viewed course: {result.Data.Name} ({result.Data.Code})", null, null);
            }
            
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseDTO.CreateCourseDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.CreateCourseAsync(dto, currentUserId);
            
            if (result.Success && result.Data != null)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "CREATE", "Course", 
                    result.Data.CourseId, $"Created course: {dto.Name} ({dto.Code})", null, dto);
            }
            
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] CourseDTO.UpdateCourseDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.UpdateCourseAsync(id, dto);
            
            if (result.Success && result.Data.UpdatedCourse != null)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "UPDATE", "Course", 
                    id, $"Updated course: {result.Data.UpdatedCourse.Name} ({result.Data.UpdatedCourse.Code})", 
                    result.Data.OldValues, dto);
            }
            
            return Ok(new { 
                Success = result.Success, 
                Data = result.Data.UpdatedCourse, 
                Message = result.Message 
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.DeleteCourseAsync(id);
            
            if (result.Success && result.Data.Deleted)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "DELETE", "Course", 
                    id, $"Deleted course: {((dynamic)result.Data.CourseInfo).Name} ({((dynamic)result.Data.CourseInfo).Code})", 
                    result.Data.CourseInfo, null);
            }
            
            return Ok(new { 
                Success = result.Success, 
                Data = result.Data.Deleted, 
                Message = result.Message 
            });
        }

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
        public async Task<IActionResult> EnrollStudent([FromRoute] long courseId, [FromBody] EnrollStudentDto dto)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.EnrollStudentInCourseAsync(courseId, dto.StudentId);
            
            if (result.Success)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "ENROLL", "Enrollment", 
                    dto.StudentId, $"Enrolled student {dto.StudentId} in course {courseId}", null, 
                    new { CourseId = courseId, StudentId = dto.StudentId });
            }
            
            return Ok(result);
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{courseId}/unenroll-student/{studentId}")]
        public async Task<IActionResult> UnenrollStudent([FromRoute] long courseId, [FromRoute] long studentId)
        {
            var currentUserId = GetCurrentUserId();
            var result = await _service.UnenrollStudentFromCourseAsync(courseId, studentId);
            
            if (result.Success)
            {
                await _activityLogService.LogEntityActionAsync(currentUserId, "UNENROLL", "Enrollment", 
                    studentId, $"Unenrolled student {studentId} from course {courseId}", 
                    new { CourseId = courseId, StudentId = studentId }, null);
            }
            
            return Ok(result);
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

    public class EnrollStudentDto
    {
        public long StudentId { get; set; }
    }
}
