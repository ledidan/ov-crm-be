using AutoMapper;
using Data.DTOs;
using Data.Entities;

public class ProductCategoryProfile : Profile
{
    public ProductCategoryProfile()
    {
        CreateMap<UpdateProductCategoryDTO, ProductCategory>().ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<ProductCategory, ProductCategoryDTO>();
    }
}
