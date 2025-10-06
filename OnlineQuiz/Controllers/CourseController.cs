using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineQuiz.Data;
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
        public async Task<ActionResult> AddCourse([FromBody] CourseModel course)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Null out navigation properties — EF will handle relationships
            course.Instructor = null;

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, new
            {
                course.CourseId,
                course.Code,
                course.Name,
                course.InstructorUserId
            });
        }

        //PUT (Update)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(long id, [FromBody] CourseModel course)
        {
            if (id != course.CourseId)
                return BadRequest("Course ID mismatch");

            course.Instructor = null; // Prevent EF circular ref
            _context.Entry(course).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Courses.Any(c => c.CourseId == id))
                    return NotFound();
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
