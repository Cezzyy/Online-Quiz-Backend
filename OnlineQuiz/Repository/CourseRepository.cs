using Microsoft.EntityFrameworkCore;
using AutoMapper;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;

namespace OnlineQuiz.Repository
{
    public class CourseRepository : ICourseRepository
    {
        private readonly OnlineQuizDbContext _context;
        private readonly IMapper _mapper;

        public CourseRepository(OnlineQuizDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetAllCoursesAsync()
        {
            var response = new ServiceResponse<IEnumerable<CourseDTO.CourseDto>>();

            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<CourseDTO.CourseDto>>(courses);
                response.Message = "Courses retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving courses: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<CourseDTO.CourseDto>> GetCourseByIdAsync(long id)
        {
            var response = new ServiceResponse<CourseDTO.CourseDto>();

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                response.Success = false;
                response.Message = "Course not found.";
                return response;
            }

            response.Data = _mapper.Map<CourseDTO.CourseDto>(course);
            return response;
        }

        public async Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto)
        {
            var response = new ServiceResponse<CourseDTO.CourseDto>();

            var model = _mapper.Map<CourseModel>(dto);
            _context.Courses.Add(model);
            await _context.SaveChangesAsync();

            response.Data = _mapper.Map<CourseDTO.CourseDto>(model);
            response.Message = "Course created successfully.";

            return response;
        }

        public async Task<ServiceResponse<CourseDTO.CourseDto>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto)
        {
            var response = new ServiceResponse<CourseDTO.CourseDto>();
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                response.Success = false;
                response.Message = "Course not found.";
                return response;
            }

            if (!string.IsNullOrWhiteSpace(dto.Code)) course.Code = dto.Code;
            if (!string.IsNullOrWhiteSpace(dto.Name)) course.Name = dto.Name;
            if (dto.InstructorUserId.HasValue) course.InstructorUserId = dto.InstructorUserId.Value;

            await _context.SaveChangesAsync();

            response.Data = _mapper.Map<CourseDTO.CourseDto>(course);
            response.Message = "Course updated successfully.";

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteCourseAsync(long id)
        {
            var response = new ServiceResponse<bool>();
            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                response.Success = false;
                response.Message = "Course not found.";
                return response;
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            response.Data = true;
            response.Message = "Course deleted successfully.";

            return response;
        }
    }
}
