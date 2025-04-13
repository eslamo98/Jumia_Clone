using Jumia_Clone.Models.DTOs.ProductImageDTOs;

namespace Jumia_Clone.Models.DTOs.CartItemDtos
{
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public List<ProductImageDto> ProductImages { get; set; }
        public int ProductId { get; set; }    
        public int VariantId { get; set; }     
        
    }
}
