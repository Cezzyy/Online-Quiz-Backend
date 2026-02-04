using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.DTOs;
using OnlineQuiz.Services;
using OnlineQuiz.IServices;

namespace OnlineQuiz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(long id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(createUserDto);
                if (!user.Success)
                    return BadRequest(user.Message);
                
                if (user.Data == null)
                    return BadRequest("Failed to create user");
                
                return CreatedAtAction(nameof(GetUser), new { id = user.Data.UserId }, user.Data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(long id, UpdateUserDto updateUserDto)
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }
            return NoContent();
        }



        [HttpGet("role/{roleName}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string roleName)
        {
            var users = await _userService.GetUsersByRoleAsync(roleName);
            return Ok(users);
        }

        [HttpPost("{id}/roles/{roleName}")]
        public async Task<IActionResult> AssignRole(long id, string roleName)
        {
            var result = await _userService.AssignRoleAsync(id, roleName);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok("Role assigned successfully");
        }

        [HttpDelete("{id}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(long id, string roleName)
        {
            var result = await _userService.RemoveRoleAsync(id, roleName);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok("Role removed successfully");
        }
    }
}