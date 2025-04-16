namespace Jumia_Clone.Models.DTOs.ProductAttributeDTOs
{
    public class UpdateProductAttributeDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string PossibleValues { get; set; }
        public bool IsRequired { get; set; }
        public bool IsFilterable { get; set; }
    }
}
