using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.UserDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken);
        Task<string> GetRefreshTokenAsync(int userId);
        Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
    
            // General user methods
            //Task<UserDto> GetUserByIdAsync(int userId);
            //Task<UserDto> GetUserByEmailAsync(string email);
            //Task<bool> EmailExistsAsync(string email);
            Task<bool> UpdateUserProfileImageAsync(int userId, string imagePath);
            Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
            Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
            Task<bool> DeleteUserAsync(int userId);

            // Customer methods
            Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(PaginationDto pagination, string searchTerm = null);
            Task<int> GetCustomersCountAsync(string searchTerm = null);
            Task<CustomerDto> GetCustomerByIdAsync(int customerId);
            Task<CustomerDto> GetCustomerByUserIdAsync(int userId);
            Task<CustomerDto> RegisterCustomerAsync(CustomerRegistrationDto registrationDto, string imagePath = null);
            Task<CustomerDto> UpdateCustomerAsync(CustomerUpdateDto updateDto, string imagePath = null);

            // Seller methods
            Task<IEnumerable<SellerDto>> GetAllSellersAsync(PaginationDto pagination, string searchTerm = null, bool? isVerified = null);
            Task<int> GetSellersCountAsync(string searchTerm = null, bool? isVerified = null);
            Task<SellerDto> GetSellerByIdAsync(int sellerId);
            Task<SellerDto> GetSellerByUserIdAsync(int userId);
            Task<SellerDto> RegisterSellerAsync(SellerRegistrationDto registrationDto, string profileImagePath = null, string businessLogoPath = null);
            Task<SellerDto> UpdateSellerAsync(SellerUpdateDto updateDto, string profileImagePath = null, string businessLogoPath = null);
            Task<SellerDto> VerifySellerAsync(int sellerId, bool verify);
            Task<IEnumerable<BasicSellerInfoDto>> GetBasicSellersInfo();

            // Admin methods
            Task<IEnumerable<AdminDto>> GetAllAdminsAsync(PaginationDto pagination, string searchTerm = null);
            Task<int> GetAdminsCountAsync(string searchTerm = null);
            Task<AdminDto> GetAdminByIdAsync(int adminId);
            Task<AdminDto> GetAdminByUserIdAsync(int userId);
            Task<AdminDto> CreateAdminAsync(AdminCreationDto creationDto, string imagePath = null);
            Task<AdminDto> UpdateAdminAsync(AdminUpdateDto updateDto, string imagePath = null);
        }
}


