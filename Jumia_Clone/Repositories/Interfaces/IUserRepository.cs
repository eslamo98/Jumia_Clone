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
    }
}
