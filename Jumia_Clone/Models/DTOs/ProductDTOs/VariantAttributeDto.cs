namespace Jumia_Clone.Models.DTOs.ProductDTOs
{
    public class VariantAttributeDto
    {
        public int VariantAttributeId { get; set; }
        public int VariantId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }
}
