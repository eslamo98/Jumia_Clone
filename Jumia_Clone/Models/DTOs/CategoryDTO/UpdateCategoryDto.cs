namespace Jumia_Clone.Models.DTOs.CategoryDTO
{
    public class UpdateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
