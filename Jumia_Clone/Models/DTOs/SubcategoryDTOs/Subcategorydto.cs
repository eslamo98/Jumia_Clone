namespace Jumia_Clone.Models.DTOs.SubcategoryDTOs
{
    public class Subcategorydto
    {
        public int SubcategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public int ProductCount { get; set; }

    }
}
