namespace Jumia_Clone.Models.DTOs.ReturnItemDTOs
{
    public class ReturnItemdto
    {
        public int ReturnItemId { get; set; }

        public int ReturnId { get; set; }

        public int OrderItemId { get; set; }

        public int Quantity { get; set; }

        public string ReturnReason { get; set; }

        public string Condition { get; set; }

        public decimal? RefundAmount { get; set; }
    }
}
