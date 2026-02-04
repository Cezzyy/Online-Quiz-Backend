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
                    .Where(c => !c.IsDeleted)
                    .Include(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(c => c.Creator)
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
                .Where(c => !c.IsDeleted)
                .Include(c => c.Instructor)
                .ThenInclude(t => t.User)
                .Include(c => c.Creator)
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

        public async Task<ServiceResponse<CourseDTO.CourseDto>> CreateCourseAsync(CourseDTO.CreateCourseDto dto, long createdByUserId)
        {
            var response = new ServiceResponse<CourseDTO.CourseDto>();

            try
            {
                var model = _mapper.Map<CourseModel>(dto);
                model.CreatedBy = createdByUserId;
                model.CreatedAt = DateTime.UtcNow;
                model.UpdatedAt = DateTime.UtcNow;
                model.IsDeleted = false;

                _context.Courses.Add(model);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var createdCourse = await _context.Courses
                    .Include(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(c => c.Creator)
                    .FirstOrDefaultAsync(c => c.CourseId == model.CourseId);

                response.Data = _mapper.Map<CourseDTO.CourseDto>(createdCourse);
                response.Message = "Course created successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating course: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<(CourseDTO.CourseDto UpdatedCourse, object OldValues)>> UpdateCourseAsync(long id, CourseDTO.UpdateCourseDto dto)
        {
            var response = new ServiceResponse<(CourseDTO.CourseDto UpdatedCourse, object OldValues)>();
            
            try
            {
                var course = await _context.Courses
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                // Capture old values for logging
                var oldValues = new
                {
                    course.Code,
                    course.Name,
                    course.InstructorUserId,
                    course.Status,
                    course.Category,
                    course.Description,
                    course.Semester,
                    course.AcademicYear,
                    course.Units,
                    course.StartDate,
                    course.EndDate,
                    course.IsPublished
                };

                // Update basic fields
                if (!string.IsNullOrWhiteSpace(dto.Code)) course.Code = dto.Code;
                if (!string.IsNullOrWhiteSpace(dto.Name)) course.Name = dto.Name;
                if (dto.InstructorUserId.HasValue) course.InstructorUserId = dto.InstructorUserId.Value;
                if (!string.IsNullOrWhiteSpace(dto.Status)) course.Status = dto.Status;
                if (dto.Category != null) course.Category = dto.Category;
                
                // Update new fields
                if (dto.Description != null) course.Description = dto.Description;
                if (dto.Semester != null) course.Semester = dto.Semester;
                if (dto.AcademicYear.HasValue) course.AcademicYear = dto.AcademicYear;
                if (dto.Units.HasValue) course.Units = dto.Units;
                if (dto.StartDate.HasValue) course.StartDate = dto.StartDate;
                if (dto.EndDate.HasValue) course.EndDate = dto.EndDate;
                if (dto.IsPublished.HasValue) course.IsPublished = dto.IsPublished.Value;
                
                course.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload with navigation properties
                var updatedCourse = await _context.Courses
                    .Include(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(c => c.Creator)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                var courseDto = _mapper.Map<CourseDTO.CourseDto>(updatedCourse);
                response.Data = (courseDto, oldValues);
                response.Message = "Course updated successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating course: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<(bool Deleted, object CourseInfo)>> DeleteCourseAsync(long id)
        {
            var response = new ServiceResponse<(bool Deleted, object CourseInfo)>();
            
            try
            {
                var course = await _context.Courses
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                // Capture course info for logging before soft deletion
                var courseInfo = new
                {
                    course.CourseId,
                    course.Code,
                    course.Name,
                    course.InstructorUserId,
                    course.Status,
                    course.Category,
                    course.Description,
                    course.Semester,
                    course.AcademicYear,
                    course.CreatedAt,
                    course.UpdatedAt,
                    course.CreatedBy
                };

                // Implement soft delete
                course.IsDeleted = true;
                course.DeletedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                response.Data = (true, courseInfo);
                response.Message = "Course deleted successfully (soft delete).";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting course: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetCoursesByInstructorAsync(long instructorId)
        {
            var response = new ServiceResponse<IEnumerable<CourseDTO.CourseDto>>();

            try
            {
                var courses = await _context.Courses
                    .Where(c => !c.IsDeleted)
                    .Include(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(c => c.Creator)
                    .Where(c => c.InstructorUserId == instructorId)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<CourseDTO.CourseDto>>(courses);
                response.Message = "Instructor courses retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving instructor courses: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<CourseDTO.CourseDto>>> GetEnrolledCoursesByStudentAsync(long studentId)
        {
            var response = new ServiceResponse<IEnumerable<CourseDTO.CourseDto>>();

            try
            {
                var courses = await _context.Enrollments
                    .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                    .ThenInclude(t => t.User)
                    .Include(e => e.Course)
                    .ThenInclude(c => c.Creator)
                    .Where(e => e.UserId == studentId && !e.Course.IsDeleted)
                    .Select(e => e.Course)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<CourseDTO.CourseDto>>(courses);
                response.Message = "Student enrolled courses retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving student courses: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<StudentDto>>> GetCourseStudentsAsync(long courseId)
        {
            var response = new ServiceResponse<IEnumerable<StudentDto>>();

            try
            {
                var students = await _context.Enrollments
                    .Include(e => e.User)
                    .ThenInclude(u => u.Student)
                    .Where(e => e.CourseId == courseId)
                    .Select(e => e.User.Student)
                    .Where(s => s != null)
                    .ToListAsync();

                response.Data = _mapper.Map<IEnumerable<StudentDto>>(students);
                response.Message = "Course students retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving course students: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> EnrollStudentInCourseAsync(long courseId, long studentId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Check if enrollment already exists
                var existingEnrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == studentId);

                if (existingEnrollment != null)
                {
                    response.Success = false;
                    response.Message = "Student is already enrolled in this course.";
                    return response;
                }

                // Check if course exists
                var course = await _context.Courses
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId);
                    
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found.";
                    return response;
                }

                // Check if student exists
                var student = await _context.Users
                    .Where(u => !u.IsDeleted)
                    .FirstOrDefaultAsync(u => u.UserId == studentId);
                    
                if (student == null)
                {
                    response.Success = false;
                    response.Message = "Student not found.";
                    return response;
                }

                var enrollment = new EnrollmentModel
                {
                    CourseId = courseId,
                    UserId = studentId,
                    EnrolledAt = DateTime.UtcNow
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Message = "Student enrolled successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error enrolling student: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> UnenrollStudentFromCourseAsync(long courseId, long studentId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == studentId);

                if (enrollment == null)
                {
                    response.Success = false;
                    response.Message = "Enrollment not found.";
                    return response;
                }

                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Message = "Student unenrolled successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error unenrolling student: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<IEnumerable<EnrollmentDTO.EnrollmentDto>>> GetCourseEnrollmentsWithDetailsAsync(long courseId)
        {
            var response = new ServiceResponse<IEnumerable<EnrollmentDTO.EnrollmentDto>>();

            try
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.User)
                    .ThenInclude(u => u.Student)
                    .Include(e => e.Course)
                    .Where(e => e.CourseId == courseId)
                    .ToListAsync();

                var enrollmentDtos = enrollments.Select(e => new EnrollmentDTO.EnrollmentDto
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    UserId = e.UserId,
                    CourseName = e.Course?.Name,
                    CourseCode = e.Course?.Code,
                    StudentName = e.User?.FullName,
                    StudentEmail = e.User?.Email,
                    EnrolledAt = e.EnrolledAt
                }).ToList();

                response.Data = enrollmentDtos;
                response.Message = "Course enrollments retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving course enrollments: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> IsStudentEnrolledInCourseAsync(long studentId, long courseId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == studentId && e.CourseId == courseId);

                response.Data = enrollment != null;
                response.Message = enrollment != null 
                    ? "Student is enrolled in the course." 
                    : "Student is not enrolled in the course.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error checking enrollment: {ex.Message}";
            }

            return response;
        }
    }
}
