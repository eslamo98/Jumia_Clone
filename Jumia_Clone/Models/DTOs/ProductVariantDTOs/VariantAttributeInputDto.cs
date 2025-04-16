using System.ComponentModel.DataAnnotations;

namespace Jumia_Clone.Models.DTOs.ProductVariantDTOs
{
    public class VariantAttributeInputDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Value { get; set; }
    }

}
