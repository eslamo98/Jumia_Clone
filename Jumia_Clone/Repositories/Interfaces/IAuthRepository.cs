using Jumia_Clone.Models.DTOs.AuthenticationDTOs;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto);
        Task<UserResponseDto> RegisterSellerAsync(RegisterUserDto registerDto, SellerRegistrationDto sellerDto);
        Task<UserResponseDto> LoginAsync(LoginDto loginDto);
        Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
        Task<bool> ChangePasswordAsync(int userId, AuthChangePasswordDto changePasswordDto);
        Task<UserResponseDto> HandleExternalAuthAsync(ExternalAuthDto externalAuth);
    }
}
