namespace Jumia_Clone.Models.DTOs.ProductImageDTOs
{
    public class UpdateProductImagesDto
    {
        public int ProductId { get; set; }
        public List<IFormFile> NewImageFiles { get; set; }
        public List<int> ImagesToDelete { get; set; }
    }
}
