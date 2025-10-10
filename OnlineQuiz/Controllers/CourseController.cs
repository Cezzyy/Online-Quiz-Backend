using OnlineQuiz.DTOs;
using OnlineQuiz.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace OnlineQuiz.Controllers
{   
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllCoursesAsync());

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById([FromRoute] long id) =>
            Ok(await _service.GetCourseByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CourseDTO.CreateCourseDto dto) =>
            Ok(await _service.CreateCourseAsync(dto));

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] CourseDTO.UpdateCourseDto dto) =>
            Ok(await _service.UpdateCourseAsync(id, dto));

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id) =>
            Ok(await _service.DeleteCourseAsync(id));
    }
}
