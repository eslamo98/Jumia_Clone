using Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs;
using Jumia_Clone.Models.DTOs.ProductImageDTOs;
using Jumia_Clone.Models.DTOs.ProductVariantDTOs;

namespace Jumia_Clone.Models.DTOs.ProductDTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string MainImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int SellerId { get; set; }
        public string SellerName { get; set; }
        public int SubcategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int RatingCount { get; set; }
        public int ReviewCount { get; set; }

        public List<ProductImageDto> Images { get; set; }
        public List<ProductVariantDto> Variants { get; set; }
        public List<ProductAttributeValueDto> AttributeValues { get; set; }
    }
}
