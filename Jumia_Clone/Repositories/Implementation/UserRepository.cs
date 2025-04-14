using AutoMapper;
using Jumia_Clone.Data;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using Jumia_Clone.Models.DTOs.UserDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Jumia_Clone.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<UserRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
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
            var user = await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Seller)
                .Include(u => u.Admin)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
                return null;

            return user;
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


        
            #region General User Methods

            //public async Task<UserDto> GetUserByIdAsync(int userId)
            //{
            //    try
            //    {
            //        var user = await _context.Users
            //            .FirstOrDefaultAsync(u => u.UserId == userId);

            //        if (user == null)
            //            return null;

            //        return _mapper.Map<UserDto>(user);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error occurred while fetching user {UserId}", userId);
            //        throw;
            //    }
            //}

            //public async Task<UserDto> GetUserByEmailAsync(string email)
            //{
            //    try
            //    {
            //        var user = await _context.Users
            //            .FirstOrDefaultAsync(u => u.Email == email);

            //        if (user == null)
            //            return null;

            //        return _mapper.Map<UserDto>(user);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error occurred while fetching user by email {Email}", email);
            //        throw;
            //    }
            //}

            //public async Task<bool> EmailExistsAsync(string email)
            //{
            //    try
            //    {
            //        return await _context.Users.AnyAsync(u => u.Email == email);
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error checking if email exists {Email}", email);
            //        throw;
            //    }
            //}

            public async Task<bool> UpdateUserProfileImageAsync(int userId, string imagePath)
            {
                try
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                        return false;

                    // Update image path
                    // Note: We're assuming there's a separate column for profile image in the User table.
                    // If not, you might need to add it to your entity class and update the database.

                    // For demo purposes, let's assume we're storing image path in a separate user_profile table
                    // or that we've extended the User entity with a ProfileImageUrl property

                    // user.ProfileImageUrl = imagePath;
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating profile image for user {UserId}", userId);
                    throw;
                }
            }

            public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
            {
                try
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                        return false;

                    // Verify current password
                    if (!VerifyPasswordHash(currentPassword, user.PasswordHash))
                    {
                        return false; // Current password is incorrect
                    }

                    // Update password
                    user.PasswordHash = HashPassword(newPassword);
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                    throw;
                }
            }

            public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                        return false;

                    user.IsActive = isActive;
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating status for user {UserId}", userId);
                    throw;
                }
            }

            public async Task<bool> DeleteUserAsync(int userId)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                        return false;

                    // Soft delete by setting IsActive to false
                    // or you could implement a hard delete if required
                    user.IsActive = false;
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(user).State = EntityState.Modified;

                    // Hard delete option:
                    // _context.Users.Remove(user);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error deleting user {UserId}", userId);
                    throw;
                }
            }

            #endregion

            #region Customer Methods

            public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(PaginationDto pagination, string searchTerm = null)
            {
                try
                {
                    IQueryable<Customer> query = _context.Customers
                        .Include(c => c.User);

                    // Apply search term if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        searchTerm = searchTerm.ToLower();
                        query = query.Where(c =>
                            c.User.Email.ToLower().Contains(searchTerm) ||
                            c.User.FirstName.ToLower().Contains(searchTerm) ||
                            c.User.LastName.ToLower().Contains(searchTerm));
                    }

                    // Apply ordering
                    query = query.OrderByDescending(c => c.User.CreatedAt);

                    // Apply pagination
                    query = query
                        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                        .Take(pagination.PageSize);

                    var customers = await query.ToListAsync();
                    return _mapper.Map<IEnumerable<CustomerDto>>(customers);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting all customers");
                    throw;
                }
            }

            public async Task<int> GetCustomersCountAsync(string searchTerm = null)
            {
                try
                {
                    IQueryable<Customer> query = _context.Customers
                        .Include(c => c.User);

                    // Apply search term if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        searchTerm = searchTerm.ToLower();
                        query = query.Where(c =>
                            c.User.Email.ToLower().Contains(searchTerm) ||
                            c.User.FirstName.ToLower().Contains(searchTerm) ||
                            c.User.LastName.ToLower().Contains(searchTerm));
                    }

                    return await query.CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting customers count");
                    throw;
                }
            }

            public async Task<CustomerDto> GetCustomerByIdAsync(int customerId)
            {
                try
                {
                    var customer = await _context.Customers
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                    if (customer == null)
                        return null;

                    return _mapper.Map<CustomerDto>(customer);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting customer by ID {CustomerId}", customerId);
                    throw;
                }
            }

            public async Task<CustomerDto> GetCustomerByUserIdAsync(int userId)
            {
                try
                {
                    var customer = await _context.Customers
                        .Include(c => c.User)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (customer == null)
                        return null;

                    return _mapper.Map<CustomerDto>(customer);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting customer by user ID {UserId}", userId);
                    throw;
                }
            }

            public async Task<CustomerDto> RegisterCustomerAsync(CustomerRegistrationDto registrationDto, string imagePath = null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create new user
                    var user = new User
                    {
                        Email = registrationDto.Email,
                        PasswordHash = HashPassword(registrationDto.Password),
                        FirstName = registrationDto.FirstName,
                        LastName = registrationDto.LastName,
                        PhoneNumber = registrationDto.PhoneNumber,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        UserType = "Customer",
                        IsActive = true
                        // ProfileImageUrl can be added if your User entity has this property
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create new customer
                    var customer = new Customer
                    {
                        UserId = user.UserId,
                        LastLogin = DateTime.UtcNow
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Return mapped DTO
                    var customerDto = _mapper.Map<CustomerDto>(customer);
                    customerDto.Email = user.Email;
                    customerDto.FirstName = user.FirstName;
                    customerDto.LastName = user.LastName;
                    customerDto.PhoneNumber = user.PhoneNumber;
                    customerDto.CreatedAt = user.CreatedAt;
                    customerDto.UpdatedAt = user.UpdatedAt;
                    customerDto.UserType = user.UserType;
                    customerDto.IsActive = user.IsActive;
                    customerDto.ProfileImageUrl = imagePath;

                    return customerDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error registering customer");
                    throw;
                }
            }

            public async Task<CustomerDto> UpdateCustomerAsync(CustomerUpdateDto updateDto, string imagePath = null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _context.Users.FindAsync(updateDto.UserId);
                    if (user == null)
                        throw new KeyNotFoundException($"User with ID {updateDto.UserId} not found");

                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.UserId == updateDto.UserId);

                    if (customer == null)
                        throw new KeyNotFoundException($"Customer for user with ID {updateDto.UserId} not found");

                    // Update user details
                    if (!string.IsNullOrEmpty(updateDto.FirstName))
                        user.FirstName = updateDto.FirstName;

                    if (!string.IsNullOrEmpty(updateDto.LastName))
                        user.LastName = updateDto.LastName;

                    if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                        user.PhoneNumber = updateDto.PhoneNumber;

                    // Update profile image if provided
                    if (imagePath != null)
                    {
                        // Assuming ProfileImageUrl is a property in your User entity
                        // user.ProfileImageUrl = imagePath;
                    }

                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(user).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Return updated customer
                    var customerDto = _mapper.Map<CustomerDto>(customer);
                    customerDto.Email = user.Email;
                    customerDto.FirstName = user.FirstName;
                    customerDto.LastName = user.LastName;
                    customerDto.PhoneNumber = user.PhoneNumber;
                    customerDto.CreatedAt = user.CreatedAt;
                    customerDto.UpdatedAt = user.UpdatedAt;
                    customerDto.UserType = user.UserType;
                    customerDto.IsActive = user.IsActive;
                    customerDto.ProfileImageUrl = imagePath ?? string.Empty; // or get from user if property exists

                    return customerDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating customer {UserId}", updateDto.UserId);
                    throw;
                }
            }

            #endregion

            #region Seller Methods

            public async Task<IEnumerable<SellerDto>> GetAllSellersAsync(PaginationDto pagination, string searchTerm = null, bool? isVerified = null)
            {
                try
                {
                    IQueryable<Seller> query = _context.Sellers
                        .Include(s => s.User);

                    // Apply search term if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        searchTerm = searchTerm.ToLower();
                        query = query.Where(s =>
                            s.User.Email.ToLower().Contains(searchTerm) ||
                            s.User.FirstName.ToLower().Contains(searchTerm) ||
                            s.User.LastName.ToLower().Contains(searchTerm) ||
                            s.BusinessName.ToLower().Contains(searchTerm));
                    }

                    // Filter by verification status if provided
                    if (isVerified.HasValue)
                    {
                        query = query.Where(s => s.IsVerified == isVerified.Value);
                    }

                    // Apply ordering
                    query = query.OrderByDescending(s => s.User.CreatedAt);

                    // Apply pagination
                    query = query
                        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                        .Take(pagination.PageSize);

                    var sellers = await query.ToListAsync();
                    return _mapper.Map<IEnumerable<SellerDto>>(sellers);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting all sellers");
                    throw;
                }
            }

            public async Task<int> GetSellersCountAsync(string searchTerm = null, bool? isVerified = null)
            {
                try
                {
                    IQueryable<Seller> query = _context.Sellers
                        .Include(s => s.User);

                    // Apply search term if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        searchTerm = searchTerm.ToLower();
                        query = query.Where(s =>
                            s.User.Email.ToLower().Contains(searchTerm) ||
                            s.User.FirstName.ToLower().Contains(searchTerm) ||
                            s.User.LastName.ToLower().Contains(searchTerm) ||
                            s.BusinessName.ToLower().Contains(searchTerm));
                    }

                    // Filter by verification status if provided
                    if (isVerified.HasValue)
                    {
                        query = query.Where(s => s.IsVerified == isVerified.Value);
                    }

                    return await query.CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting sellers count");
                    throw;
                }
            }

            public async Task<SellerDto> GetSellerByIdAsync(int sellerId)
            {
                try
                {
                    var seller = await _context.Sellers
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.SellerId == sellerId);

                    if (seller == null)
                        return null;

                    return _mapper.Map<SellerDto>(seller);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting seller by ID {SellerId}", sellerId);
                    throw;
                }
            }

            public async Task<SellerDto> GetSellerByUserIdAsync(int userId)
            {
                try
                {
                    var seller = await _context.Sellers
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.UserId == userId);

                    if (seller == null)
                        return null;

                    return _mapper.Map<SellerDto>(seller);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting seller by user ID {UserId}", userId);
                    throw;
                }
            }

            public async Task<SellerDto> RegisterSellerAsync(SellerRegistrationDto registrationDto, string profileImagePath = null, string businessLogoPath = null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create new user
                    var user = new User
                    {
                        Email = registrationDto.Email,
                        PasswordHash = HashPassword(registrationDto.Password),
                        FirstName = registrationDto.FirstName,
                        LastName = registrationDto.LastName,
                        PhoneNumber = registrationDto.PhoneNumber,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        UserType = "Seller",
                        IsActive = true
                        // ProfileImageUrl can be added if your User entity has this property
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create new seller
                    var seller = new Seller
                    {
                        UserId = user.UserId,
                        BusinessName = registrationDto.BusinessName,
                        BusinessDescription = registrationDto.BusinessDescription,
                        BusinessLogo = businessLogoPath,
                        IsVerified = false,
                        Rating = 0
                    };

                    _context.Sellers.Add(seller);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Return mapped DTO
                    var sellerDto = _mapper.Map<SellerDto>(seller);
                    sellerDto.Email = user.Email;
                    sellerDto.FirstName = user.FirstName;
                    sellerDto.LastName = user.LastName;
                    sellerDto.PhoneNumber = user.PhoneNumber;
                    sellerDto.CreatedAt = user.CreatedAt;
                    sellerDto.UpdatedAt = user.UpdatedAt;
                    sellerDto.UserType = user.UserType;
                    sellerDto.IsActive = user.IsActive;
                    sellerDto.ProfileImageUrl = profileImagePath;

                    return sellerDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error registering seller");
                    throw;
                }
            }

            public async Task<SellerDto> UpdateSellerAsync(SellerUpdateDto updateDto, string profileImagePath = null, string businessLogoPath = null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _context.Users.FindAsync(updateDto.UserId);
                    if (user == null)
                        throw new KeyNotFoundException($"User with ID {updateDto.UserId} not found");

                    var seller = await _context.Sellers
                        .FirstOrDefaultAsync(s => s.UserId == updateDto.UserId);

                    if (seller == null)
                        throw new KeyNotFoundException($"Seller for user with ID {updateDto.UserId} not found");

                    // Update user details
                    if (!string.IsNullOrEmpty(updateDto.FirstName))
                        user.FirstName = updateDto.FirstName;

                    if (!string.IsNullOrEmpty(updateDto.LastName))
                        user.LastName = updateDto.LastName;

                    if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                        user.PhoneNumber = updateDto.PhoneNumber;

                    // Update profile image if provided
                    if (profileImagePath != null)
                    {
                        // Assuming ProfileImageUrl is a property in your User entity
                        // user.ProfileImageUrl = profileImagePath;
                    }

                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(user).State = EntityState.Modified;

                    // Update seller details
                    if (!string.IsNullOrEmpty(updateDto.BusinessName))
                        seller.BusinessName = updateDto.BusinessName;

                    if (updateDto.BusinessDescription != null)
                        seller.BusinessDescription = updateDto.BusinessDescription;

                    if (businessLogoPath != null)
                        seller.BusinessLogo = businessLogoPath;

                    _context.Entry(seller).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Return updated seller
                    var sellerDto = _mapper.Map<SellerDto>(seller);
                    sellerDto.Email = user.Email;
                    sellerDto.FirstName = user.FirstName;
                    sellerDto.LastName = user.LastName;
                    sellerDto.PhoneNumber = user.PhoneNumber;
                    sellerDto.CreatedAt = user.CreatedAt;
                    sellerDto.UpdatedAt = user.UpdatedAt;
                    sellerDto.UserType = user.UserType;
                    sellerDto.IsActive = user.IsActive;
                    sellerDto.ProfileImageUrl = profileImagePath ?? string.Empty; // or get from user if property exists

                    return sellerDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating seller {UserId}", updateDto.UserId);
                    throw;
                }
            }

            public async Task<SellerDto> VerifySellerAsync(int sellerId, bool verify)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var seller = await _context.Sellers
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.SellerId == sellerId);

                    if (seller == null)
                        return null;

                    seller.IsVerified = verify;
                    if (verify)
                    {
                        seller.VerifiedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        seller.VerifiedAt = null;
                    }

                    _context.Entry(seller).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return _mapper.Map<SellerDto>(seller);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error verifying seller {SellerId}", sellerId);
                    throw;
                }
            }

            #endregion

            #region Admin Methods

            public async Task<IEnumerable<AdminDto>> GetAllAdminsAsync(PaginationDto pagination, string searchTerm = null)
            {
                try
                {
                    IQueryable<Admin> query = _context.Admins
                        .Include(a => a.User);

                    // Apply search term if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        searchTerm = searchTerm.ToLower();
                        query = query.Where(a =>
                            a.User.Email.ToLower().Contains(searchTerm) ||
                            a.User.FirstName.ToLower().Contains(searchTerm) ||
                            a.User.LastName.ToLower().Contains(searchTerm) ||
                            a.Role.ToLower().Contains(searchTerm));
                    }

                    // Apply ordering
                    query = query.OrderByDescending(a => a.User.CreatedAt);

                    // Apply pagination
                    query = query
                        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                        .Take(pagination.PageSize);

                    var admins = await query.ToListAsync();
                    return _mapper.Map<IEnumerable<AdminDto>>(admins);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting all admins");
                    throw;
                }
            }

            public async Task<int> GetAdminsCountAsync(string searchTerm = null)
            {
                try
                {
                    IQueryable<Admin> query = _context.Admins
                        .Include(a => a.User);

                    // Apply search term if provided
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        searchTerm = searchTerm.ToLower();
                        query = query.Where(a =>
                            a.User.Email.ToLower().Contains(searchTerm) ||
                            a.User.FirstName.ToLower().Contains(searchTerm) ||
                            a.User.LastName.ToLower().Contains(searchTerm) ||
                            a.Role.ToLower().Contains(searchTerm));
                    }

                    return await query.CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting admins count");
                    throw;
                }
            }

            public async Task<AdminDto> GetAdminByIdAsync(int adminId)
            {
                try
                {
                    var admin = await _context.Admins
                        .Include(a => a.User)
                        .FirstOrDefaultAsync(a => a.AdminId == adminId);

                    if (admin == null)
                        return null;

                    return _mapper.Map<AdminDto>(admin);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting admin by ID {AdminId}", adminId);
                    throw;
                }
            }

            public async Task<AdminDto> GetAdminByUserIdAsync(int userId)
            {
                try
                {
                    var admin = await _context.Admins
                        .Include(a => a.User)
                        .FirstOrDefaultAsync(a => a.UserId == userId);

                    if (admin == null)
                        return null;

                    return _mapper.Map<AdminDto>(admin);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting admin by user ID {UserId}", userId);
                    throw;
                }
            }

            public async Task<AdminDto> CreateAdminAsync(AdminCreationDto creationDto, string imagePath = null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create new user
                    var user = new User
                    {
                        Email = creationDto.Email,
                        PasswordHash = HashPassword(creationDto.Password),
                        FirstName = creationDto.FirstName,
                        LastName = creationDto.LastName,
                        PhoneNumber = creationDto.PhoneNumber,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        UserType = "Admin",
                        IsActive = true,
                        ProfileImageUrl = imagePath
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create new admin
                    var admin = new Admin
                    {
                        UserId = user.UserId,
                        Role = creationDto.Role,
                        Permissions = creationDto.Permissions
                    };

                    _context.Admins.Add(admin);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Return mapped DTO
                    var adminDto = _mapper.Map<AdminDto>(admin);
                    adminDto.Email = user.Email;
                    adminDto.FirstName = user.FirstName;
                    adminDto.LastName = user.LastName;
                    adminDto.PhoneNumber = user.PhoneNumber;
                    adminDto.CreatedAt = user.CreatedAt;
                    adminDto.UpdatedAt = user.UpdatedAt;
                    adminDto.UserType = user.UserType;
                    adminDto.IsActive = user.IsActive;
                    adminDto.ProfileImageUrl = imagePath;

                    return adminDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error creating admin");
                    throw;
                }
            }

            public async Task<AdminDto> UpdateAdminAsync(AdminUpdateDto updateDto, string imagePath = null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _context.Users.FindAsync(updateDto.UserId);
                    if (user == null)
                        throw new KeyNotFoundException($"User with ID {updateDto.UserId} not found");

                    var admin = await _context.Admins
                        .FirstOrDefaultAsync(a => a.UserId == updateDto.UserId);

                    if (admin == null)
                        throw new KeyNotFoundException($"Admin for user with ID {updateDto.UserId} not found");

                    // Update user details
                    if (!string.IsNullOrEmpty(updateDto.FirstName))
                        user.FirstName = updateDto.FirstName;

                    if (!string.IsNullOrEmpty(updateDto.LastName))
                        user.LastName = updateDto.LastName;

                    if (!string.IsNullOrEmpty(updateDto.PhoneNumber))
                        user.PhoneNumber = updateDto.PhoneNumber;

                    // Update profile image if provided
                    if (imagePath != null)
                    {
                        // Assuming ProfileImageUrl is a property in your User entity
                        // user.ProfileImageUrl = imagePath;
                    }

                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Entry(user).State = EntityState.Modified;

                    // Update admin details
                    if (!string.IsNullOrEmpty(updateDto.Role))
                        admin.Role = updateDto.Role;

                    if (updateDto.Permissions != null)
                        admin.Permissions = updateDto.Permissions;

                    _context.Entry(admin).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Return updated admin
                    var adminDto = _mapper.Map<AdminDto>(admin);
                    adminDto.Email = user.Email;
                    adminDto.FirstName = user.FirstName;
                    adminDto.LastName = user.LastName;
                    adminDto.PhoneNumber = user.PhoneNumber;
                    adminDto.CreatedAt = user.CreatedAt;
                    adminDto.UpdatedAt = user.UpdatedAt;
                    adminDto.UserType = user.UserType;
                    adminDto.IsActive = user.IsActive;
                    adminDto.ProfileImageUrl = imagePath ?? string.Empty; // or get from user if property exists

                    return adminDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating admin {UserId}", updateDto.UserId);
                    throw;
                }
            }

            #endregion

            #region Helper Methods

            private string HashPassword(string password)
            {
                // For production, consider using a more secure password hashing mechanism
                // like BCrypt.Net, Identity PasswordHasher, or similar
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }

            private bool VerifyPasswordHash(string password, string storedHash)
            {
                // Compute hash of provided password and compare with stored hash
                string computedHash = HashPassword(password);
                return storedHash.Equals(computedHash);
            }

            #endregion
        }
    }

