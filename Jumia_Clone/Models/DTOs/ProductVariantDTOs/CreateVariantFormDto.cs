using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class CreateVariantFormDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string VariantName { get; set; }

        [Required]
        [Range(0.01, 99999999.99)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal? DiscountPercentage { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(50)]
        public string Sku { get; set; }

        public IFormFile VariantImage { get; set; }

        public bool? IsDefault { get; set; }

        public bool? IsAvailable { get; set; }

        // For handling attributes as form fields
        // In the controller, you'll need to parse these into VariantAttributeInputDto objects
        public string AttributeNames { get; set; } // Comma-separated list of attribute names
        public string AttributeValues { get; set; } // Comma-separated list of attribute values
    }
}
