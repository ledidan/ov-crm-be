using AutoMapper;
using Data.DTOs;
using Data.Entities;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<UpdateProductDTO, Product>().ForMember(dest => dest.Id, opt => opt.Ignore());
        CreateMap<Product, ProductDTO>();
    }
}
