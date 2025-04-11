namespace Jumia_Clone.Models.DTOs.ReturnRequestDTOs
{
    public class ReturnRequestdto
    {
        public int ReturnId { get; set; }

        public int SuborderId { get; set; }

        public int CustomerId { get; set; }

        public string ReturnReason { get; set; }

        public string ReturnStatus { get; set; }

        public DateTime? RequestedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? ReceivedAt { get; set; }

        public decimal? RefundAmount { get; set; }

        public DateTime? RefundedAt { get; set; }

        public string TrackingNumber { get; set; }

        public string Comments { get; set; }
    }
}
