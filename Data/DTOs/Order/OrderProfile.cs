using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.MongoModels;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderDTO, Order>(); // Maps OrderDTO to Order entity
        CreateMap<Order, OptionalOrderDTO>();
        CreateMap<OptionalOrderDTO, Order>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<UpdateOrderDTO, Order>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<OrderDTO, Order>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Order, OrderDTO>();
        CreateMap<OrderDetailDTO, OrderDetails>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore ID if it's managed by MongoDB
            .ReverseMap();
    }
}
