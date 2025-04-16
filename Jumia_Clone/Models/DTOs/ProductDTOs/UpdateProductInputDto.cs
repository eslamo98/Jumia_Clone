using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs;
using System.Text.Json.Serialization;

namespace Jumia_Clone.Models.DTOs.ProductDTOs
{
    public class UpdateProductInputDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public int StockQuantity { get; set; }
        public int SubcategoryId { get; set; }
        public IFormFile MainImageFile { get; set; }
        public string ProductAttributeValuesJson { get; set; }
        //public string ProductVariantsJson { get; set; }
        [JsonIgnore]
        public List<CreateProductAttributeValueDto> AttributeValues { get; set; }
    }
}
