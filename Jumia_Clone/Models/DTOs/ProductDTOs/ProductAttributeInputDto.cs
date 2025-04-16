using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.ProductDTOs
{
    public class ProductAttributeInputDto
    {
        [Required]
        public int AttributeId { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
