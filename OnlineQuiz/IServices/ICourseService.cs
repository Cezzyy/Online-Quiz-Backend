using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IServices
{
    public interface ICourseService
    {
        Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseDTO.CourseDto>> GetCourseByIdAsync(long id);
        Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto);
        Task<ServiceResponse<CourseDTO.CourseDto>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto);
        Task<ServiceResponse<bool>> DeleteCourseAsync(long id);
    }
}
