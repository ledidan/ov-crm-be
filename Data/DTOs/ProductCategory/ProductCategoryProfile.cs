using AutoMapper;
using Data.DTOs;
using Data.Entities;

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        CreateMap<UpdateProductCategoryDTO, ProductCategory>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ProductCategory, ProductCategoryDTO>();
    }
}
