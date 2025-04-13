using AutoMapper;
using Jumia_Clone.Models.DTOs.AddressDTO;
using Jumia_Clone.Models.DTOs.AddressDTOs;
using Jumia_Clone.Models.DTOs.OrderDTOs;
using Jumia_Clone.Models.Entities;
using Jumia_Clone.Repositories.Interfaces;

namespace Jumia_Clone.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Address to AddressDto
            CreateMap<Address, AddressDto>()
                .ForMember(dest => dest.IsDefault,
                    opt => opt.MapFrom(src => src.IsDefault ?? false));

            // CreateAddressRequest to Address
            CreateMap<CreateAddressRequest, Address>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            // UpdateAddressRequest to Address
            CreateMap<UpdateAddressRequest, Address>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            // AddressFilterRequest to AddressFilterParameters
            CreateMap<AddressFilterRequest, AddressFilterParameters>();

            // Add the missing mappings
            CreateMap<CreateAddressInputDto, Address>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            CreateMap<UpdateAddressInputDto, Address>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            // Entity to DTO mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.SubOrders, opt => opt.MapFrom(src => src.SubOrders));

            CreateMap<SubOrder, SubOrderDto>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemDto>();

            // DTO to Entity mappings for create operations
            CreateMap<CreateOrderInputDto, Order>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(_ => "pending"))
                .ForMember(dest => dest.SubOrders, opt => opt.Ignore());

            CreateMap<CreateSubOrderInputDto, SubOrder>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "pending"))
                .ForMember(dest => dest.StatusUpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore());

            CreateMap<CreateOrderItemInputDto, OrderItem>();

            // DTO to Entity mappings for update operations
            CreateMap<UpdateOrderInputDto, Order>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateSubOrderInputDto, SubOrder>()
                .ForMember(dest => dest.StatusUpdatedAt, opt => opt.MapFrom<StatusUpdatedResolver>())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
                    srcMember != null && (!(srcMember is string) || !string.IsNullOrEmpty((string)srcMember))));
        }
    }

    public class StatusUpdatedResolver : IValueResolver<UpdateSubOrderInputDto, SubOrder, DateTime?>
    {
        public DateTime? Resolve(UpdateSubOrderInputDto source, SubOrder destination, DateTime? destMember, ResolutionContext context)
        {
            return !string.IsNullOrEmpty(source.Status) ? DateTime.UtcNow : destination.StatusUpdatedAt;
        }
    }
}