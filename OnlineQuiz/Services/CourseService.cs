using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models.Response;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace OnlineQuiz.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourseService(ICourseRepository courseRepository, IHttpContextAccessor httpContextAccessor)
        {
            _courseRepository = courseRepository;
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

        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetAllCoursesAsync() =>
            await _courseRepository.GetAllCoursesAsync();

        public async Task<ServiceResponse<CourseDTO.CourseDto>> GetCourseByIdAsync(long id) =>
            await _courseRepository.GetCourseByIdAsync(id);

        public async Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                return await _courseRepository.CreateCourseAsync(dto, currentUserId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return new ServiceResponse<CourseDTO.CourseDto>(ex.Message);
            }
        }

        public async Task<ServiceResponse<CourseDTO.CourseDto>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto)
        {
            var result = await _courseRepository.UpdateCourseAsync(id, dto);
            if (!result.Success)
                return new ServiceResponse<CourseDTO.CourseDto>(result.Message ?? "Failed to update course");
            return new ServiceResponse<CourseDTO.CourseDto>(result.Data.UpdatedCourse);
        }

        public async Task<ServiceResponse<bool>> DeleteCourseAsync(long id)
        {
            var result = await _courseRepository.DeleteCourseAsync(id);
            if (!result.Success)
                return new ServiceResponse<bool>(result.Message ?? "Failed to delete course");
            return new ServiceResponse<bool>(result.Data.Deleted);
        }

        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetCoursesByInstructorAsync(long instructorId) =>
            await _courseRepository.GetCoursesByInstructorAsync(instructorId);

        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetCoursesByStudentAsync(long studentId) =>
            await _courseRepository.GetEnrolledCoursesByStudentAsync(studentId);

        public async Task<ServiceResponse<EnrollmentDTO.EnrollmentDto>> EnrollStudentAsync(EnrollmentDTO.CreateEnrollmentDto dto)
        {
            var result = await _courseRepository.EnrollStudentInCourseAsync(dto.CourseId, dto.UserId);
            if (!result.Success)
                return new ServiceResponse<EnrollmentDTO.EnrollmentDto>(result.Message ?? "Failed to enroll student");
            
            // Return a simple enrollment DTO
            return new ServiceResponse<EnrollmentDTO.EnrollmentDto>(new EnrollmentDTO.EnrollmentDto
            {
                CourseId = dto.CourseId,
                UserId = dto.UserId,
                EnrolledAt = DateTime.UtcNow
            });
        }

        public async Task<ServiceResponse<bool>> UnenrollStudentAsync(long userId, long courseId)
        {
            return await _courseRepository.UnenrollStudentFromCourseAsync(courseId, userId);
        }

        public Task<ServiceResponse<IEnumerable<EnrollmentDTO.EnrollmentDto>>> GetCourseEnrollmentsAsync(long courseId)
        {
            // This method needs to be implemented in the repository
            // For now, return empty list
            return Task.FromResult(new ServiceResponse<IEnumerable<EnrollmentDTO.EnrollmentDto>>(new List<EnrollmentDTO.EnrollmentDto>()));
        }

        public Task<ServiceResponse<bool>> IsStudentEnrolledAsync(long userId, long courseId)
        {
            // This method needs to be implemented in the repository
            // For now, return false
            return Task.FromResult(new ServiceResponse<bool>(false));
        }
    }
}
