namespace Jumia_Clone.Models.DTOs.CartItemDtos
{
    public class UpdateCartItemDto
    {
        public int ProductId { get; set; }    
        public int VariantId { get; set; }     
        public int Quantity { get; set; }
        public int CartItemId { get; set; }
    }
}
