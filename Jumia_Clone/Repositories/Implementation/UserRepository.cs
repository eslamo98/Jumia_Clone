using Jumia_Clone.Data;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Jumia_Clone.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Seller)
                .Include(u => u.Admin)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Seller)
                .Include(u => u.Admin)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }

        // Note: In a real application, you would store refresh tokens in a separate table
        // This is a simplified implementation for demonstration purposes
        private readonly Dictionary<int, string> _refreshTokens = new();

        public Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken)
        {
            _refreshTokens[userId] = refreshToken;
            return Task.FromResult(true);
        }

        public Task<string> GetRefreshTokenAsync(int userId)
        {
            _refreshTokens.TryGetValue(userId, out var token);
            return Task.FromResult(token);
        }

        public Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            _refreshTokens.TryGetValue(userId, out var storedToken);
            return Task.FromResult(storedToken == refreshToken);
        }
    }
}
