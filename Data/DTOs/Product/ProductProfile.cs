using AutoMapper;
using Data.DTOs;
using Data.Entities;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<UpdateProductDTO, Product>().ForMember(dest => dest.Id, opt => opt.Ignore()).ForAllMembers(
            opt => opt.Condition((src, dest, srcMember) => srcMember != null)
        );
        CreateMap<Product, ProductDTO>();
    }
}
