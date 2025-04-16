namespace Jumia_Clone.Models.Constants
{
    /// <summary>
    /// Defines the possible approval statuses for products
    /// </summary>
    public static class ProductApprovalStatus
    {
        // Standard Approval Statuses
        public const string Pending = "pending";
        public const string Approved = "approved";
        public const string Rejected = "rejected";
        public const string PendingReview = "pending_review";
        public const string Deleted = "deleted";

        // Additional statuses if needed
        public const string Suspended = "suspended";
        public const string Archived = "archived";

        /// <summary>
        /// Validates if the given status is a valid product approval status
        /// </summary>
        public static bool IsValidStatus(string status)
        {
            return new[]
            {
                Pending,
                Approved,
                Rejected,
                PendingReview,
                Deleted,
                Suspended,
                Archived
            }.Contains(status);
        }

        /// <summary>
        /// Checks if the status represents an active product status
        /// </summary>
        public static bool IsActive(string status)
        {
            return status == Approved;
        }

        /// <summary>
        /// Checks if the status prevents the product from being displayed or sold
        /// </summary>
        public static bool IsInactive(string status)
        {
            return new[]
            {
                Pending,
                Rejected,
                PendingReview,
                Deleted,
                Suspended
            }.Contains(status);
        }

        /// <summary>
        /// Gets all statuses that represent a reviewable state
        /// </summary>
        public static string[] GetReviewableStatuses()
        {
            return new[]
            {
                Pending,
                PendingReview
            };
        }

        /// <summary>
        /// Determines if a status allows product modifications
        /// </summary>
        public static bool CanModify(string status)
        {
            return status == Pending ||
                   status == PendingReview ||
                   status == Approved;
        }
    }
}
