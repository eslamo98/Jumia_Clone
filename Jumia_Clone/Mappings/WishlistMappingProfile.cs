using AutoMapper;
using Jumia_Clone.Models.DTOs.WishlistItemDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.MappingProfiles
{
    public class WishlistMappingProfile : Profile
    {
        public WishlistMappingProfile()
        {
            // Map Wishlist entity to WishlistDto
            CreateMap<Wishlist, WishlistDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
                .ForMember(dest => dest.ItemsCount, opt => opt.Ignore())
                .ForMember(dest => dest.WishlistItems, opt => opt.Ignore());

            // Map WishlistItem entity to WishlistItemDto
            CreateMap<WishlistItem, WishlistItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductDescription, opt => opt.Ignore())
                .ForMember(dest => dest.BasePrice, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentPrice, opt => opt.Ignore())
                .ForMember(dest => dest.MainImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
                .ForMember(dest => dest.StockQuantity, opt => opt.Ignore());
        }
    }
}