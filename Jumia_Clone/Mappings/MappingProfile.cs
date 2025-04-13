using AutoMapper;
using Jumia_Clone.Models.DTOs.AddressDTO;
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
        }
    }
}
