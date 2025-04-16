namespace Jumia_Clone.Models.DTOs.ProductImageDTOs
{
    public class CreateProductImageDto
    {
        public int ProductId { get; set; }
        public List<IFormFile> ImageFiles { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
