using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repo;

        public CourseService(ICourseRepository repo)
        {
            _repo = repo;
        }

        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetAllCoursesAsync() =>
            await _repo.GetAllCoursesAsync();

        public async Task<ServiceResponse<CourseDTO.CourseDto>> GetCourseByIdAsync(long id) =>
            await _repo.GetCourseByIdAsync(id);

        public async Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto) =>
            await _repo.CreateCourseAsync(dto);

        public async Task<ServiceResponse<CourseDTO.CourseDto>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto) =>
            await _repo.UpdateCourseAsync(id, dto);

        public async Task<ServiceResponse<bool>> DeleteCourseAsync(long id) =>
            await _repo.DeleteCourseAsync(id);
    }
}
