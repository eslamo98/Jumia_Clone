using Jumia_Clone.Models.DTOs;
using Jumia_Clone.Models.DTOs.AuthenticationDTOs;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jumia_Clone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                var result = await _authRepository.RegisterUserAsync(registerDto);
                return Ok(new ApiResponse<UserResponseDto>(result, "Registration successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(null) { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("register-seller")]
        public async Task<IActionResult> RegisterSeller([FromBody] SellerRegistrationRequestDto request)
        {
            try
            {
                var result = await _authRepository.RegisterSellerAsync(request.User, request.Seller);
                return Ok(new ApiResponse<UserResponseDto>(result, "Seller registration successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(null) { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authRepository.LoginAsync(loginDto);
                return Ok(new ApiResponse<UserResponseDto>(result, "Login successful"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(null) { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authRepository.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                return Ok(new ApiResponse<TokenResponseDto>(result));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(null) { Success = false, Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _authRepository.ChangePasswordAsync(userId, changePasswordDto);
                return Ok(new ApiResponse<object>(null, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object>(null) { Success = false, Message = ex.Message });
            }
        }
    }

    // Combined DTO for seller registration
    public class SellerRegistrationRequestDto
    {
        public RegisterUserDto User { get; set; }
        public SellerRegistrationDto Seller { get; set; }
    }
}