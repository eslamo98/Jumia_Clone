namespace Jumia_Clone.Models.DTOs.ProductAttributeDTOs
{
    public class ProductAttributeDto
    {
        public int AttributeId { get; set; }
        public int SubcategoryId { get; set; }
        public string SubcategoryName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string PossibleValues { get; set; }
        public bool IsRequired { get; set; }
        public bool IsFilterable { get; set; }
    }
}
