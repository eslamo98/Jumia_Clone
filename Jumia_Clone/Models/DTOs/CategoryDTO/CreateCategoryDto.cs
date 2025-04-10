using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jumia_Clone.Models.DTOs.CategoryDTO
{
    public class CreateCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
