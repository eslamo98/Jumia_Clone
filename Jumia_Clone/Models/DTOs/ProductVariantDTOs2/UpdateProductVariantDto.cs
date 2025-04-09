using Jumia_Clone.Models.DTOs.VariantAttributeDTOs;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs2
{
    public class UpdateProductVariantDto
    {
        public int VariantId { get; set; }
        public string VariantName { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int StockQuantity { get; set; }
        public string Sku { get; set; }
        public IFormFile VariantImageFile { get; set; }
        public bool? IsDefault { get; set; }
        public List<CreateVariantAttributeDto> Attributes { get; set; }
    }
}
