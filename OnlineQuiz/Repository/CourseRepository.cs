using Microsoft.EntityFrameworkCore;
using AutoMapper;
using OnlineQuiz.Data;
using OnlineQuiz.Models;
using OnlineQuiz.Models.Response;
using OnlineQuiz.DTOs;
using OnlineQuiz.IRepository;

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

        public async Task<ServiceResponse<IEnumerable<CourseDto>>> GetAllCoursesAsync()
        {
            var response = new ServiceResponse<IEnumerable<CourseDto>>();
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Instructor)
                    .ThenInclude(i => i.User)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<CourseDto>>(courses);
                response.Message = "Courses retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving courses: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<CourseDto>> GetCourseByIdAsync(long id)
        {
            var response = new ServiceResponse<CourseDto>();
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .ThenInclude(i => i.User)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                response.Data = _mapper.Map<CourseDto>(course);
                response.Message = "Course retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving course: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<CourseDto>> GetCourseByCodeAsync(string code)
        {
            var response = new ServiceResponse<CourseDto>();
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Instructor)
                    .ThenInclude(i => i.User)
                    .FirstOrDefaultAsync(c => c.Code == code);

                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                response.Data = _mapper.Map<CourseDto>(course);
                response.Message = "Course retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving course: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<CourseDto>> CreateCourseAsync(CreateCourseDto dto)
        {
            var response = new ServiceResponse<CourseDto>();
            try
            {
                var instructorExists = await _context.Teachers.AnyAsync(t => t.UserId == dto.InstructorUserId);
                if (!instructorExists)
                {
                    response.Success = false;
                    response.Message = "Instructor not found.";
                    return response;
                }

                if (await _context.Courses.AnyAsync(c => c.Code == dto.Code))
                {
                    response.Success = false;
                    response.Message = "A course with that code already exists.";
                    return response;
                }

                var course = _mapper.Map<CourseModel>(dto);
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<CourseDto>(course);
                response.Message = "Course created successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating course: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<CourseDto>> UpdateCourseAsync(long id, UpdateCourseDto dto)
        {
            var response = new ServiceResponse<CourseDto>();
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                if (!string.IsNullOrWhiteSpace(dto.Code))
                    course.Code = dto.Code;
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    course.Name = dto.Name;
                if (dto.InstructorUserId.HasValue)
                    course.InstructorUserId = dto.InstructorUserId.Value;

                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<CourseDto>(course);
                response.Message = "Course updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating course: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse> DeleteCourseAsync(long id)
        {
            var response = new ServiceResponse();
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                response.Message = "Course deleted successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting course: {ex.Message}";
            }
            return response;
        }
    }
}
