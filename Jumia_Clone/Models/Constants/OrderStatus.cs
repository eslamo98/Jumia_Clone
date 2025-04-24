namespace Jumia_Clone.Models.Constants
{
    public static class OrderStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Canceled = "Canceled";
        public const string Returned = "Returned";
        public const string Failed = "Failed";

        public static readonly string[] AllowedValues = new[]
        {
                Pending,
                Processing,
                Shipped,
                Delivered,
                Canceled,
                Returned,
                Failed
            };
    }

    public static class PaymentStatus
    {
        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Refunded = "Refunded";
        public const string PartiallyRefunded = "PartiallyRefunded";

        public static readonly string[] AllowedValues = new[]
        {
                Pending,
                Completed,
                Failed,
                Refunded,
                PartiallyRefunded
            };
    }

    public static class SubOrderStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Canceled = "Canceled";
        public const string Returned = "Returned";

        public static readonly string[] AllowedValues = new[]
        {
                Pending,
                Processing,
                Shipped,
                Delivered,
                Canceled,
                Returned
            };
    }
    public static class CouponType
    {
        public const string Fixed = "Fixed";
        public const string Percentage = "Percentage";
    }
    public static class PaymentMethods
    {
        public const string CreditCard = "CreditCard";
        public const string DebitCard = "DebitCard";
        public const string PayPal = "PayPal";
        public const string BankTransfer = "BankTransfer";
        public const string CashOnDelivery = "CashOnDelivery";

        public static readonly string[] AllowedValues = new[]
        {
                CreditCard,
                DebitCard,
                PayPal,
                BankTransfer,
                CashOnDelivery
            };
    }
}
