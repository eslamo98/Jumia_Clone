using Jumia_Clone.Models.DTOs.AuthenticationDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Jumia_Clone.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Jumia_Clone.Repositories.Implementation
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthRepository(
            IUserRepository userRepository,
            JwtService jwtService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                throw new Exception("Email is already registered");
            }

            // Create new user
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

            // Save user to database
            await _userRepository.CreateUserAsync(user);

            // Create customer record if user type is customer
            if (registerDto.UserType == "customer")
            {
                var customer = new Customer
                {
                    UserId = user.UserId,
                    LastLogin = DateTime.UtcNow
                };

                // In a real implementation, you would add the customer to the database here
            }

            // Generate JWT token and refresh token
            var token = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            await _userRepository.SaveRefreshTokenAsync(user.UserId, refreshToken);

            // Return user response with token
            return new UserResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserType = user.UserType,
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<UserResponseDto> RegisterSellerAsync(RegisterUserDto registerDto, SellerRegistrationDto sellerDto)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                throw new Exception("Email is already registered");
            }

            // Create new user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                UserType = "seller", // Force user type to seller
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Save user to database
            await _userRepository.CreateUserAsync(user);

            // Create seller record
            var seller = new Seller
            {
                UserId = user.UserId,
                BusinessName = sellerDto.BusinessName,
                BusinessDescription = sellerDto.BusinessDescription,
                BusinessLogo = sellerDto.BusinessLogo,
                IsVerified = false,
                Rating = 0.0
            };

            // In a real implementation, you would add the seller to the database here

            // Generate JWT token and refresh token
            var token = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            await _userRepository.SaveRefreshTokenAsync(user.UserId, refreshToken);

            // Return user response with token
            return new UserResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserType = user.UserType,
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<UserResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Get user by email
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

            // Check if user exists and password is correct
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new Exception("Invalid email or password");
            }

            // Check if user is active
            if (user.IsActive == false)
            {
                throw new Exception("User is inactive");
            }

            // Generate JWT token and refresh token
            var token = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            await _userRepository.SaveRefreshTokenAsync(user.UserId, refreshToken);

            // Update last login for customer
            if (user.UserType == "customer" && user.Customer != null)
            {
                user.Customer.LastLogin = DateTime.UtcNow;
                // In a real implementation, you would update the customer in the database
            }

            // Return user response with token
            return new UserResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserType = user.UserType,
                Token = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // Get principal from expired token
            var principal = _jwtService.GetPrincipalFromExpiredToken(refreshToken);

            // Get user ID from claims
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new Exception("Invalid token");
            }

            var userId = int.Parse(userIdClaim.Value);

            // Validate refresh token
            var isValidRefreshToken = await _userRepository.ValidateRefreshTokenAsync(userId, refreshToken);
            if (!isValidRefreshToken)
            {
                throw new Exception("Invalid refresh token");
            }

            // Get user
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Generate new JWT token and refresh token
            var newToken = _jwtService.GenerateJwtToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Save new refresh token
            await _userRepository.SaveRefreshTokenAsync(userId, newRefreshToken);

            // Return new tokens
            return new TokenResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            // Get user
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Verify current password
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new Exception("Current password is incorrect");
            }

            // Update password
            user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Save changes
            return await _userRepository.UpdateUserAsync(user);
        }

        // Helper methods for password hashing and verification
        private static string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Combine salt and hash
            var result = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, result, 0, salt.Length);
            Array.Copy(hash, 0, result, salt.Length, hash.Length);

            return Convert.ToBase64String(result);
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            // Decode stored hash
            var hashBytes = Convert.FromBase64String(storedHash);

            // Extract salt and hash
            var salt = new byte[64]; // HMACSHA512 key size
            var hash = new byte[hashBytes.Length - salt.Length];

            Array.Copy(hashBytes, 0, salt, 0, salt.Length);
            Array.Copy(hashBytes, salt.Length, hash, 0, hash.Length);

            // Compute hash for provided password
            using var hmac = new HMACSHA512(salt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Compare hashes
            for (var i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
