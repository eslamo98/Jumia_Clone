using System.Text.Json.Serialization;

namespace Jumia_Clone.Models.DTOs.CategoryDTO
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int SubcategoryCount { get; set; }
        [JsonIgnore]
        public IFormFile ImageFile { get; set; }
        
    }
}
