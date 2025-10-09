using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;

namespace OnlineQuiz.IRepository
{
    public interface ICourseRepository
    {
        Task<ServiceResponse<IEnumerable<CourseDto>>> GetAllCoursesAsync();
        Task<ServiceResponse<CourseDto>> GetCourseByIdAsync(long courseId);
        Task<ServiceResponse<CourseDto>> GetCourseByCodeAsync(string code);
        Task<ServiceResponse<CourseDto>> CreateCourseAsync(CreateCourseDto createCourseDto);
        Task<ServiceResponse<CourseDto>> UpdateCourseAsync(long courseId, UpdateCourseDto updateCourseDto);
        Task<ServiceResponse> DeleteCourseAsync(long courseId);
    }
}
