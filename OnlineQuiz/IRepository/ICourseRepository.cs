using OnlineQuiz.DTOs;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.IRepository
{
    public interface ICourseRepository
    {
        Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseDTO.CourseDto>> GetCourseByIdAsync(long id);
        Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto);
        Task<ServiceResponse<CourseDTO.CourseDto>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto);
        Task<ServiceResponse<bool>> DeleteCourseAsync(long id);
    }
}
