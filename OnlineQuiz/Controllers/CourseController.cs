using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OnlineQuiz.Data;
using OnlineQuiz.DTOs;
using OnlineQuiz.Models;

namespace OnlineQuiz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly OnlineQuizDbContext _context;

        public CourseController(OnlineQuizDbContext context)
        {
            _context = context;
        }

        //GET all courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseModel>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Select(c => new
                {
                    c.CourseId,
                    c.Code,
                    c.Name,
                    c.InstructorUserId,
                    Instructor = new
                    {
                        c.Instructor.UserId,
                        c.Instructor.Department
                    }
                })
                .ToListAsync();

            return Ok(courses);
        }

        //GET a single course
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseModel>> GetCourse(long id)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            return Ok(new
            {
                course.CourseId,
                course.Code,
                course.Name,
                course.InstructorUserId,
                Instructor = new
                {
                    course.Instructor.UserId,
                    course.Instructor.Department
                }
            });
        }

        //POST a new course
        [HttpPost]
        public async Task<IActionResult> AddCourse([FromBody] CourseCreationDto courseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var teacher = await _context.Teachers
                                        .Include(t => t.User)
                                        .FirstOrDefaultAsync(t => t.UserId == courseDto.InstructorUserId);

            if (teacher == null)
            {
                ModelState.AddModelError("InstructorUserId", "The specified instructor (Teacher with this UserId) does not exist.");
                return BadRequest(ModelState);
            }

            // Map DTO to Model
            var course = new CourseModel
            {
                Code = courseDto.Code,
                Name = courseDto.Name,
                InstructorUserId = courseDto.InstructorUserId 
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var responseDto = new CourseDto
            {
                CourseId = course.CourseId,
                Code = course.Code,
                Name = course.Name,
                InstructorUserId = course.InstructorUserId,
                InstructorName = teacher.User?.FullName ?? "Unknown"
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, responseDto);
        }

        //PUT (Update)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseCreationDto courseDto)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Course ID provided in the route.");
            }


            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null)
            {
                return NotFound();
            }
            var teacherExist = await _context.Teachers.AnyAsync(t => t.UserId == courseDto.InstructorUserId);
            if (!teacherExist)
            {
                ModelState.AddModelError("InstructorUserId", "The specified instructor (Teacher with this UserId) does not exist.");
                return BadRequest(ModelState);
            }
            if (existingCourse.Code != courseDto.Code)
            {
                var courseWithSameCodeExist = await _context.Courses.AnyAsync(c => c.Code == courseDto.Code && c.CourseId != id);

                if (courseWithSameCodeExist)
                {
                    ModelState.AddModelError("Code", $"A course with code '{courseDto.Code}' already exists.");
                    return Conflict(ModelState);
                }
            }

            // Update properties
            existingCourse.Code = courseDto.Code;
            existingCourse.Name = courseDto.Name;
            existingCourse.InstructorUserId = courseDto.InstructorUserId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Courses.AnyAsync(c => c.CourseId == id))
                {
                    return NotFound(); 
                }
                throw;
            }

            return NoContent();
        }
        
        //DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(long id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Course deleted successfully" });
        }
    }
}
