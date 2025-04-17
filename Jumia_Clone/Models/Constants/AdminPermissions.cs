namespace Jumia_Clone.Models.Constants
{
    
    /// <summary>
    /// Defines granular permissions for admin actions
    /// </summary>
    public static class AdminPermissions
    {
        public const string All = "All";

        // Product Management Permissions
        public const string ProductView = "Product.View";
        public const string ProductCreate = "Product.Create";
        public const string ProductUpdate = "Product.Update";
        public const string ProductDelete = "Product.Delete";
        public const string ProductApprove = "Product.Approve";

        // Seller Management Permissions
        public const string SellerView = "Seller.View";
        public const string SellerCreate = "Seller.Create";
        public const string SellerUpdate = "Seller.Update";
        public const string SellerVerify = "Seller.Verify";
        public const string SellerSuspend = "Seller.Suspend";

        // Order Management Permissions
        public const string OrderView = "Order.View";
        public const string OrderUpdate = "Order.Update";
        public const string OrderCancel = "Order.Cancel";

        // Category Management Permissions
        public const string CategoryView = "Category.View";
        public const string CategoryCreate = "Category.Create";
        public const string CategoryUpdate = "Category.Update";
        public const string CategoryDelete = "Category.Delete";

        // User Management Permissions
        public const string UserView = "User.View";
        public const string UserCreate = "User.Create";
        public const string UserUpdate = "User.Update";
        public const string UserDelete = "User.Delete";
        public const string UserBan = "User.Ban";

        // Customer Management Permissions
        public const string CustomerView = "Customer.View";
        public const string CustomerSupport = "Customer.Support";

        // Attribute Management Permissions
        public const string AttributeView = "Attribute.View";
        public const string AttributeCreate = "Attribute.Create";
        public const string AttributeUpdate = "Attribute.Update";
        public const string AttributeDelete = "Attribute.Delete";

        // Analytics and Reporting Permissions
        public const string ReportView = "Report.View";
        public const string ReportGenerate = "Report.Generate";

        // Financial Permissions
        public const string PayoutView = "Payout.View";
        public const string PayoutProcess = "Payout.Process";

        // Review and Ratings Permissions
        public const string ReviewModerate = "Review.Moderate";

        // Marketing Permissions
        public const string CouponManage = "Coupon.Manage";
        public const string PromotionCreate = "Promotion.Create";

        /// <summary>
        /// Validates if the given permission is a valid admin permission
        /// </summary>
        public static bool IsValidPermission(string permission)
        {
            return typeof(AdminPermissions)
                .GetFields()
                .Where(f => f.IsPublic && f.IsStatic && f.FieldType == typeof(string))
                .Select(f => f.GetValue(null) as string)
                .Contains(permission);
        }

        /// <summary>
        /// Gets all product-related permissions
        /// </summary>
        public static string[] GetProductPermissions()
        {
            return new[]
            {
            ProductView,
            ProductCreate,
            ProductUpdate,
            ProductDelete,
            ProductApprove
        };
        }

        /// <summary>
        /// Gets all seller-related permissions
        /// </summary>
        public static string[] GetSellerPermissions()
        {
            return new[]
            {
            SellerView,
            SellerCreate,
            SellerUpdate,
            SellerVerify,
            SellerSuspend
        };
        }
    }
    
}
