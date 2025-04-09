using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.AuthenticationDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Jumia_Clone.Repositories.Implementation
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthRepository(ApplicationDbContext context, IUserRepository userRepository, IConfiguration configuration)
        {
            _context = context;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                throw new Exception("Email already exists");
            }

            // Create user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                UserType = registerDto.UserType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create user first to get UserId
                await _userRepository.CreateUserAsync(user);

                // Create corresponding customer or seller record
                if (registerDto.UserType.ToLower() == "customer")
                {
                    var customer = new Customer
                    {
                        UserId = user.UserId,
                        LastLogin = DateTime.UtcNow
                    };

                    await _context.Customers.AddAsync(customer);
                    await _context.SaveChangesAsync();
                }
                // If seller, it should be registered through RegisterSellerAsync method
                else if (registerDto.UserType.ToLower() == "seller")
                {
                    throw new Exception("Please use the register-seller endpoint to register as a seller");
                }

                await transaction.CommitAsync();

                // Generate tokens
                var tokenResponse = GenerateTokens(user);

                // Save refresh token
                await _userRepository.SaveRefreshTokenAsync(user.UserId, tokenResponse.RefreshToken);

                // Map to response DTO
                return new UserResponseDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserType = user.UserType,
                    Token = tokenResponse.Token,
                    RefreshToken = tokenResponse.RefreshToken
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserResponseDto> RegisterSellerAsync(RegisterUserDto registerDto, SellerRegistrationDto sellerDto)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                throw new Exception("Email already exists");
            }

            // Verify user type is seller
            if (registerDto.UserType.ToLower() != "seller")
            {
                throw new Exception("User type must be 'seller' for seller registration");
            }

            // Create user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                UserType = "seller",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create user first to get UserId
                await _userRepository.CreateUserAsync(user);

                // Create seller record
                var seller = new Seller
                {
                    UserId = user.UserId,
                    BusinessName = sellerDto.BusinessName,
                    BusinessDescription = sellerDto.BusinessDescription,
                    BusinessLogo = sellerDto.BusinessLogo,
                    IsVerified = false, // Sellers need verification
                    Rating = 0.0
                };

                await _context.Sellers.AddAsync(seller);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Generate tokens
                var tokenResponse = GenerateTokens(user);

                // Save refresh token
                await _userRepository.SaveRefreshTokenAsync(user.UserId, tokenResponse.RefreshToken);

                // Map to response DTO
                return new UserResponseDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserType = user.UserType,
                    Token = tokenResponse.Token,
                    RefreshToken = tokenResponse.RefreshToken
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Get user by email
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid credentials");
            }

            if (user.IsActive != true)
            {
                throw new Exception("User account is inactive");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update last login for customer
                if (user.UserType.ToLower() == "customer" && user.Customer != null)
                {
                    user.Customer.LastLogin = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Generate tokens
                var tokenResponse = GenerateTokens(user);

                // Save refresh token
                await _userRepository.SaveRefreshTokenAsync(user.UserId, tokenResponse.RefreshToken);

                await transaction.CommitAsync();

                // Map to response DTO
                return new UserResponseDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserType = user.UserType,
                    Token = tokenResponse.Token,
                    RefreshToken = tokenResponse.RefreshToken
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // Extract user ID from the refresh token (assuming JWT format)
            var principal = GetPrincipalFromExpiredToken(refreshToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Validate the refresh token
            if (!await _userRepository.ValidateRefreshTokenAsync(userId, refreshToken))
            {
                throw new Exception("Invalid refresh token");
            }

            // Get user
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.IsActive != true)
            {
                throw new Exception("User not found or inactive");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Generate new tokens
                var tokenResponse = GenerateTokens(user);

                // Save new refresh token
                await _userRepository.SaveRefreshTokenAsync(user.UserId, tokenResponse.RefreshToken);

                await transaction.CommitAsync();

                return tokenResponse;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Verify old password
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new Exception("Current password is incorrect");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Set new password
                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                bool result = await _userRepository.UpdateUserAsync(user);

                await transaction.CommitAsync();

                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #region Helper Methods

        private string HashPassword(string password)
        {
            // For real applications, use a proper password hashing library like BCrypt.Net-Next
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Combine salt and hash
            var hashBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // For real applications, use a proper password hashing library like BCrypt.Net-Next
            var hashBytes = Convert.FromBase64String(storedHash);

            // Extract salt (first 64 bytes)
            var salt = new byte[64];
            Array.Copy(hashBytes, 0, salt, 0, 64);

            // Hash the input password with the extracted salt
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Compare the computed hash with the stored hash
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != hashBytes[64 + i])
                {
                    return false;
                }
            }

            return true;
        }

        private TokenResponseDto GenerateTokens(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserType),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create access token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                signingCredentials: creds
            );

            // Create refresh token (simple GUID for demo purposes)
            // In production, use a more secure method and store with expiration
            var refreshToken = Guid.NewGuid().ToString();

            return new TokenResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken
            };
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false // Don't validate lifetime for refresh tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        #endregion
    }
}