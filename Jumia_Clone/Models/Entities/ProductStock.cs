namespace Jumia_Clone.Models.Entities
{
    public class ProductStock
    {
        public int StockId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public string AdjustmentReason { get; set; }
        public DateTime AdjustedAt { get; set; }
        public int AdjustedByUserId { get; set; }

        public virtual Product Product { get; set; }
        public virtual ProductVariant Variant { get; set; }
        public virtual User AdjustedBy { get; set; }
    }
}
