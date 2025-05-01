using Jumia_Clone.Data;
using Jumia_Clone.Helpers;
using Jumia_Clone.Models.Constants;
using Jumia_Clone.Models.DTOs.AuthenticationDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
                PasswordHash = PasswordHelpers.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                UserType = UserRoles.Customer,
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
                if (registerDto.UserType.ToLower() == UserRoles.Customer.ToLower())
                {
                    var customer = new Customer
                    {
                        UserId = user.UserId,
                        LastLogin = DateTime.UtcNow
                    };

                    await _context.Customers.AddAsync(customer);
                    await _context.SaveChangesAsync();
                    user.UserId = customer.UserId;
                }
                // If seller, it should be registered through RegisterSellerAsync method
                else if (registerDto.UserType.ToLower() == UserRoles.Seller.ToLower())
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
            if (registerDto.UserType.ToLower() != UserRoles.Seller.ToLower())
            {
                throw new Exception("User type must be 'seller' for seller registration");
            }

            // Create user
            var user = new User
            {
                Email = registerDto.Email,
                PasswordHash = PasswordHelpers.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                UserType = UserRoles.Seller,
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
                    UserId = seller.UserId,
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

            if (user == null || !PasswordHelpers.VerifyPassword(loginDto.Password, user.PasswordHash))
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
                if (user.UserType.ToLower() == UserRoles.Customer.ToLower() && user.Customer != null)
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
                    RefreshToken = tokenResponse.RefreshToken,
                    EntityId = user.UserType.ToLower() == UserRoles.Customer.ToLower() ? user.Customer.CustomerId : user.UserType.ToLower() == UserRoles.Admin.ToLower() ? user.Admin.AdminId : user.Seller.SellerId
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

        public async Task<bool> ChangePasswordAsync(int userId, AuthChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // Verify old password
            if (!PasswordHelpers.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new Exception("Current password is incorrect");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Set new password
                user.PasswordHash = PasswordHelpers.HashPassword(changePasswordDto.NewPassword);
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

        public async Task<UserResponseDto> HandleExternalAuthAsync(ExternalAuthDto externalAuth)
        {
            var user = await _userRepository.GetUserByEmailAsync(externalAuth.Email);

            if (user == null && !externalAuth.IsNewUser)
            {
                throw new Exception("User not found. Please register first.");
            }

            if (user == null && externalAuth.IsNewUser)
            {
                // Generate a random password for external auth users
                var randomPassword = Guid.NewGuid().ToString();
                var passwordHash = PasswordHelpers.HashPassword(randomPassword);

                // Create new user 
                user = new User
                {
                    Email = externalAuth.Email,
                    FirstName = externalAuth.Name.Split(' ')[0],
                    LastName = externalAuth.Name.Split(' ').Length > 1 ? externalAuth.Name.Split(' ')[1] : "",
                    UserType = UserRoles.Customer,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ProfileImageUrl = externalAuth.PhotoUrl,
                    PasswordHash = passwordHash  // Add the password hash
                };

                await _userRepository.CreateUserAsync(user);

                var customer = new Customer
                {
                    UserId = user.UserId,
                    LastLogin = DateTime.UtcNow
                };

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();
            }
            else if (user != null && externalAuth.IsNewUser)
            {
                throw new Exception("User already exists. Please login instead.");
            }

            // Update last login for existing users 
            if (!externalAuth.IsNewUser && user.Customer != null)
            {
                user.Customer.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Generate tokens 
            var tokenResponse = GenerateTokens(user);

            // Save refresh token 
            await _userRepository.SaveRefreshTokenAsync(user.UserId, tokenResponse.RefreshToken);

            return new UserResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserType = user.UserType,
                Token = tokenResponse.Token,
                RefreshToken = tokenResponse.RefreshToken,
                EntityId = user.UserType.ToLower() == UserRoles.Customer.ToLower() ? user.Customer.CustomerId : user.UserType.ToLower() == UserRoles.Admin.ToLower() ? user.Admin.AdminId : user.Seller.SellerId
            };
        }
        #region Helper Methods
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

            // Use the correct configuration paths
            var jwtKey = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured. Please check your application settings.");
            }

            var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "default-issuer";
            var jwtAudience = _configuration["JwtSettings:Audience"] ?? "default-audience";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Get expiry from config or default to 60 minutes
            int expiryInMinutes = 60;
            if (int.TryParse(_configuration["JwtSettings:ExpiryInMinutes"], out int configExpiry))
            {
                expiryInMinutes = configExpiry;
            }

            // Create access token
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                signingCredentials: creds
            );

            // Create refresh token
            var refreshToken = Guid.NewGuid().ToString();

            return new TokenResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken
            };
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtKey = _configuration["JwtSettings:SecretKey"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Secret Key is not configured. Please check your application settings.");
            }

            var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "default-issuer";
            var jwtAudience = _configuration["JwtSettings:Audience"] ?? "default-audience";

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
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