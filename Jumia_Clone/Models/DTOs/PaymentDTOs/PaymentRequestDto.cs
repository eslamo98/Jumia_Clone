namespace Jumia_Clone.Models.DTOs.PaymentDTOs {
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string ReturnUrl { get; set; }
    }
}