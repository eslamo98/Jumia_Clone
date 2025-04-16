using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class BatchUpdateStockDto
    {
        [Required]
        public List<VariantStockUpdate> Updates { get; set; } = new List<VariantStockUpdate>();

        public class VariantStockUpdate
        {
            [Required]
            public int VariantId { get; set; }

            [Required]
            [Range(0, int.MaxValue)]
            public int StockQuantity { get; set; }
        }
    }
}
