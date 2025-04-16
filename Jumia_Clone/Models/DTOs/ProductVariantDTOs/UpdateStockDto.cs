using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class UpdateStockDto
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}
