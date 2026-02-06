using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IServices
{
    public interface ICourseService
    {
        Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseDTO.PagedCoursesDto>> GetCoursesWithFilterAsync(CourseDTO.CourseFilterDto filter);
        Task<ServiceResponse<CourseDTO.CourseDto>> GetCourseByIdAsync(long id);
        Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto);
        Task<ServiceResponse<CourseDTO.CourseDto>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto);
        Task<ServiceResponse<bool>> DeleteCourseAsync(long id);
        
        // Course management methods
        Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetCoursesByInstructorAsync(long instructorId);
        Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetCoursesByStudentAsync(long studentId);
        Task<ServiceResponse<EnrollmentDTO.EnrollmentDto>> EnrollStudentAsync(EnrollmentDTO.CreateEnrollmentDto dto);
        Task<ServiceResponse<bool>> UnenrollStudentAsync(long userId, long courseId);
        Task<ServiceResponse<IEnumerable<EnrollmentDTO.EnrollmentDto>>> GetCourseEnrollmentsAsync(long courseId);
        Task<ServiceResponse<bool>> IsStudentEnrolledAsync(long userId, long courseId);
        
        // Statistics
        Task<ServiceResponse<CourseDTO.CourseStatisticsDto>> GetCourseStatisticsAsync(long courseId);
    }
}
