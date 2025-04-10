namespace Jumia_Clone.Models.DTOs.ProductImageDTOs
{
    public class ProductImageDto
    {
        public int ImageId { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
    }
}
