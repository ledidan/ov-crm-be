
using AutoMapper;
using Data.DTOs;
using Data.Entities;

public class CustomerCareTicketProfile : Profile
{
    public CustomerCareTicketProfile()
    {
        CreateMap<CustomerCare, CustomerCareTicketDTO>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<CustomerCareTicketDTO, CustomerCare>();
    }
}
