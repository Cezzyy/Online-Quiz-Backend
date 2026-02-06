using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IRepository
{
    public interface ICourseRepository
    {
        Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseDTO.PagedCoursesDto>> GetCoursesWithFilterAsync(CourseDTO.CourseFilterDto filter);
        Task<ServiceResponse<CourseDTO.CourseDto>> GetCourseByIdAsync(long id);
        Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto, long createdByUserId);
        Task<ServiceResponse<(CourseDTO.CourseDto UpdatedCourse, object OldValues)>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto);
        Task<ServiceResponse<(bool Deleted, object CourseInfo)>> DeleteCourseAsync(long id);
        
        // New methods for course management workflow
        Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetCoursesByInstructorAsync(long instructorId);
        Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetEnrolledCoursesByStudentAsync(long studentId);
        Task<ServiceResponse<IEnumerable<StudentDto>>> GetCourseStudentsAsync(long courseId);
        Task<ServiceResponse<bool>> EnrollStudentInCourseAsync(long courseId, long studentId);
        Task<ServiceResponse<bool>> UnenrollStudentFromCourseAsync(long courseId, long studentId);
        Task<ServiceResponse<IEnumerable<EnrollmentDTO.EnrollmentDto>>> GetCourseEnrollmentsWithDetailsAsync(long courseId);
        Task<ServiceResponse<bool>> IsStudentEnrolledInCourseAsync(long studentId, long courseId);
        
        // Statistics
        Task<ServiceResponse<CourseDTO.CourseStatisticsDto>> GetCourseStatisticsAsync(long courseId);
    }
}
