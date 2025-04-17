namespace Jumia_Clone.Models.Constants
{
    /// <summary>
    /// Defines the system-wide user roles
    /// </summary>
    public static class UserRoles
    {
        // Basic Roles
        public const string Admin = "Admin";
        public const string Seller = "Seller";
        public const string Customer = "Customer";
        public const string Affiliate = "Affiliate";

        // Additional Specialized Roles (if needed)
        public const string Support = "Support";
        public const string Moderator = "Moderator";

        /// <summary>
        /// Validates if the given role is a valid system role
        /// </summary>
        public static bool IsValidRole(string role)
        {
            return new[]
            {
                Admin,
                Seller,
                Customer,
                Affiliate,
                Support,
                Moderator
            }.Contains(role);
        }
    }
}
