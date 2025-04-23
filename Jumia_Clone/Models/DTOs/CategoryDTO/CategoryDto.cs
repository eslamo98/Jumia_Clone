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
        public int ProductCount { get; set; }
        public string Status { get => this.IsActive ? "Active" : "InActive"; set => this.Status = value; }
        [JsonIgnore]
        public IFormFile ImageFile { get; set; }
        
    }
}
