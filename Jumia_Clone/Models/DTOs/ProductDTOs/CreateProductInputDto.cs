using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs2;
using System.Text.Json.Serialization;

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
        public List<IFormFile> AdditionalImageFiles { get; set; }
        public string ProductAttributeValuesJson { get; set; }
        public string ProductVariantsJson { get; set; }
        public bool HasVariants { get; set; }

        [JsonIgnore]
        public List<CreateProductAttributeValueDto> AttributeValues { get; set; }

        [JsonIgnore]
        public List<CreateProductBaseVariantDto> Variants { get; set; }
    }
}
