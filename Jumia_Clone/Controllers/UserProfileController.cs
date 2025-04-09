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

                if (!result.Success)
                    return NotFound(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null
                    });

                return Ok(new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "User retrieved successfully",
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving the user",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _repository.GetAllUsersAsync();

                if (!result.Success)
                    return BadRequest(new ApiResponse<List<UserResponseDto>>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null
                    });

                return Ok(new ApiResponse<List<UserResponseDto>>
                {
                    Success = true,
                    Message = "Users retrieved successfully",
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while retrieving users",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid user data",
                        ErrorMessages = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    });

                var result = await _repository.CreateUserAsync(dto);

                if (!result.Success)
                    return BadRequest(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null
                    });

                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = result.Data.Id },
                    new ApiResponse<UserResponseDto>
                    {
                        Success = true,
                        Message = "User created successfully",
                        Data = result.Data
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while creating the user",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserProfileDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid user data",
                        ErrorMessages = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    });

                var result = await _repository.UpdateUserAsync(id, dto);

                if (!result.Success)
                    return NotFound(new ApiResponse<UserResponseDto>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null
                    });

                return Ok(new ApiResponse<UserResponseDto>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while updating the user",
                    ErrorMessages = new[] { ex.Message }
                });
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
                        return BadRequest(new ApiResponse<string>
                        {
                            Success = false,
                            Message = result.Message,
                            Data = null
                        });

                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "User deleted successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while deleting the user",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiErrorResponse
                    {
                        Message = "Invalid password data",
                        ErrorMessages = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    });

                var result = await _repository.ChangePasswordAsync(id, dto);

                if (!result.Success)
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null
                    });

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Password changed successfully",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Message = "An error occurred while changing the password",
                    ErrorMessages = new[] { ex.Message }
                });
            }
        }
    }
}