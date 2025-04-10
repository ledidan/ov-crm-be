using AutoMapper;
using Data.DTOs;
using Data.Entities;

public class SupportTicketProfile : Profile
{
    public SupportTicketProfile()
    {
        CreateMap<SupportTicket, SupportTicketDTO>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<SupportTicketDTO, SupportTicket>();
    }
}
