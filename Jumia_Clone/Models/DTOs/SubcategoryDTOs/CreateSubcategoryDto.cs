namespace Jumia_Clone.Models.DTOs.SubcategoryDTOs
{
    public class CreateSubcategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public IFormFile ImageFile { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
    }
}
