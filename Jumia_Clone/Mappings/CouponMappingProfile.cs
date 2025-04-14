using AutoMapper;
using Jumia_Clone.Models.DTOs.CouponDTOs;
using Jumia_Clone.Models.Entities;

namespace Jumia_Clone.MappingProfiles
{
    public class CouponMappingProfile : Profile
    {
        public CouponMappingProfile()
        {
            // Map Coupon entity to CouponDto
            CreateMap<Coupon, CouponDto>();

            // Map CreateCouponDto to Coupon entity
            CreateMap<CreateCouponDto, Coupon>();

            // Map UpdateCouponDto to Coupon entity
            CreateMap<UpdateCouponDto, Coupon>();

            // Map UserCoupon entity to UserCouponDto
            CreateMap<UserCoupon, UserCouponDto>();
        }
    }
}