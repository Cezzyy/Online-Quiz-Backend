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

        public async Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto, long createdByUserId) =>
            await _repo.CreateCourseAsync(dto, createdByUserId);

        public async Task<ServiceResponse<CourseDTO.CourseDto>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto) =>
            await _repo.UpdateCourseAsync(id, dto);

        public async Task<ServiceResponse<bool>> DeleteCourseAsync(long id) =>
            await _repo.DeleteCourseAsync(id);

        // New methods for course management workflow
        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetCoursesByInstructorAsync(long instructorId) =>
            await _repo.GetCoursesByInstructorAsync(instructorId);

        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetEnrolledCoursesByStudentAsync(long studentId) =>
            await _repo.GetEnrolledCoursesByStudentAsync(studentId);

        public async Task<ServiceResponse<IEnumerable<StudentDto>>> GetCourseStudentsAsync(long courseId) =>
            await _repo.GetCourseStudentsAsync(courseId);

        public async Task<ServiceResponse<bool>> EnrollStudentInCourseAsync(long courseId, long studentId) =>
            await _repo.EnrollStudentInCourseAsync(courseId, studentId);

        public async Task<ServiceResponse<bool>> UnenrollStudentFromCourseAsync(long courseId, long studentId) =>
            await _repo.UnenrollStudentFromCourseAsync(courseId, studentId);
    }
}
