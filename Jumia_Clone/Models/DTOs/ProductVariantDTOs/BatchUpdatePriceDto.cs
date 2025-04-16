using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class BatchUpdatePriceDto
    {
        [Required]
        public List<VariantPriceUpdate> Updates { get; set; } = new List<VariantPriceUpdate>();

        public class VariantPriceUpdate
        {
            [Required]
            public int VariantId { get; set; }

            [Required]
            [Range(0.01, 99999999.99)]
            public decimal Price { get; set; }

            [Range(0, 100)]
            public decimal? DiscountPercentage { get; set; }
        }
    }
}
