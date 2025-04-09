using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs2;

namespace Jumia_Clone.Models.DTOs.ProductDTOs
{
    public class CreateProductInputDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int StockQuantity { get; set; }
        public int SubcategoryId { get; set; }
        public int SellerId { get; set; }
        public IFormFile MainImageFile { get; set; }
        public List<CreateProductAttributeValueDto> AttributeValues { get; set; }
        public List<CreateProductVariantDto> Variants { get; set; }
    }
}
