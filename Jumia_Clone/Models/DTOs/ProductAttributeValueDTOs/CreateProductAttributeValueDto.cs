namespace Jumia_Clone.Models.DTOs.ProductAttributeValueDTOs
{
    public class CreateProductAttributeValueDto
    {
        public int ProductId { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; }
    }
}
