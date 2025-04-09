using Jumia_Clone.Models.DTOs.AuthenticationDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Jumia_Clone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileRepository _repository;

        public UserProfileController(IUserProfileRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var result = await _repository.GetUserByIdAsync(id);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserResponseDto>(null, $"Error retrieving user: {ex.Message}", false));
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _repository.GetAllUsersAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<UserResponseDto>>(null, $"Error retrieving users: {ex.Message}", false));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<UserResponseDto>(null, "Invalid user data", false));

                var result = await _repository.CreateUserAsync(dto);
                return result.Success ? CreatedAtAction(nameof(GetUserById), new { id = result.Data.Id }, result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserResponseDto>(null, $"Error creating user: {ex.Message}", false));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<UserResponseDto>(null, "Invalid user data", false));

                var result = await _repository.UpdateUserAsync(id, dto);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<UserResponseDto>(null, $"Error updating user: {ex.Message}", false));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _repository.DeleteUserAsync(id);
                if (!result.Success)
                {
                    if (result.Message.Contains("active orders") || result.Message.Contains("active products"))
                        return BadRequest(result); // 400 for business rule violation
                    return NotFound(result); // 404 if user not found
                }
                return Ok(result); // 200 on success
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(null, $"Error deleting user: {ex.Message}", false));
            }
        }

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<string>(null, "Invalid password data", false));

                var result = await _repository.ChangePasswordAsync(id, dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(null, $"Error changing password: {ex.Message}", false));
            }
        }
    }
}