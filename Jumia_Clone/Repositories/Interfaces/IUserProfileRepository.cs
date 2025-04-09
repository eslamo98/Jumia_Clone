using Jumia_Clone.Models.DTOs.AuthenticationDTOs;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.AuthenticationDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int id);
        Task<ApiResponse<List<UserResponseDto>>> GetAllUsersAsync();
        Task<ApiResponse<UserResponseDto>> CreateUserAsync(UpdateUserProfileDto userDto);
        Task<ApiResponse<UserResponseDto>> UpdateUserAsync(int id, UpdateUserProfileDto userDto);
        Task<ApiResponse<string>> DeleteUserAsync(int id);
        Task<ApiResponse<string>> ChangePasswordAsync(int id, ChangePasswordDto dto);
    }
}
