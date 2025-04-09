using Jumia_Clone.Models.DTOs.CartDTOs;
using Jumia_Clone.Models.DTOs.CartItemDtos;
using Jumia_Clone.Models.DTOs.GeneralDTOs;
using System.Threading.Tasks;

namespace Jumia_Clone.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<CartDto> GetOrCreateCartAsync(int customerId, PaginationDto pagination);
        Task<CartItemDto> AddItemToCartAsync(int customerId, AddItemToCartDto addItemDto);
        Task<CartItemDto> UpdateCartItemAsync(UpdateCartItemDto updateItemDto);
        Task<bool> RemoveItemFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(int customerId);
        Task<CartItemDto> GetCartItemAsync(int cartItemId);
    }
}