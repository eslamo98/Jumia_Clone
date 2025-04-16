namespace Jumia_Clone.Models.DTOs.ProductAttributeDTOs
{
    public class CreateProductAttributeDto
    {
        public int SubcategoryId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string PossibleValues { get; set; }
        public bool IsRequired { get; set; } = false;
        public bool IsFilterable { get; set; } = true;
    }
}
