using AutoMapper;
using Data.DTOs;
using Data.Entities;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderDTO, Order>(); // Maps OrderDTO to Order entity
        CreateMap<Order, ContactOrderDTO>();
        CreateMap<ContactOrderDTO, Order>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<OrderDTO, Order>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Order, OrderDTO>();
    }
}
