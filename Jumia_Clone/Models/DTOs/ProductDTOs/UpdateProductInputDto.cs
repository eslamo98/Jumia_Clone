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
        //public string ProductAttributesJson { get; set; }
        //public string ProductVariantsJson { get; set; }

        //// Deserialized objects (not bound directly from form)
        //[JsonIgnore]
        //public List<ProductAttributeInputDto> ProductAttributes { get; set; } = new List<ProductAttributeInputDto>();

        //[JsonIgnore]
        //public List<UpdateProductVariantDto> ProductVariants { get; set; } = new List<UpdateProductVariantDto>();
    }
}
