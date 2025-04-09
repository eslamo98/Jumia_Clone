namespace Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs
{
    public class ProductAttributeValueDto
    {
        public int ValueId { get; set; }
        public int ProductId { get; set; }
        public int AttributeId { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
    }
}
