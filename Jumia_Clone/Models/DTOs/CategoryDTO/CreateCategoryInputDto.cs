namespace Jumia_Clone.Models.DTOs.CategoryDTO
{
    public class CreateCategoryInputDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
