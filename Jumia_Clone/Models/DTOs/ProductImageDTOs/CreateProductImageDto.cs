namespace Jumia_Clone.Models.DTOs.ProductImageDTOs
{
    public class CreateProductImageDto
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public int? DisplayOrder { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
