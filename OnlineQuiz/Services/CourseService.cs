using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.IServices;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repo;
        public CourseService(ICourseRepository repo) => _repo = repo;

        public Task<ServiceResponse<IEnumerable<CourseDto>>> GetAllCoursesAsync() => _repo.GetAllCoursesAsync();
        public Task<ServiceResponse<CourseDto>> GetCourseByIdAsync(long id) => _repo.GetCourseByIdAsync(id);
        public Task<ServiceResponse<CourseDto>> GetCourseByCodeAsync(string code) => _repo.GetCourseByCodeAsync(code);
        public Task<ServiceResponse<CourseDto>> CreateCourseAsync(CreateCourseDto dto) => _repo.CreateCourseAsync(dto);
        public Task<ServiceResponse<CourseDto>> UpdateCourseAsync(long id, UpdateCourseDto dto) => _repo.UpdateCourseAsync(id, dto);
        public Task<ServiceResponse> DeleteCourseAsync(long id) => _repo.DeleteCourseAsync(id);
    }
}
