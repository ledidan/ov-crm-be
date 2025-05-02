using AutoMapper;
using Data.Entities;

namespace Data.DTOs
{
    public class ContactProfile : Profile
    {
        public ContactProfile()
        {
            CreateMap<ContactDTO, Contact>()
    .ForMember(dest => dest.PartnerId, opt => opt.Ignore());
            CreateMap<Contact, AllContactDTO>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Contact, ContactDTO>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AllContactDTO, Contact>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

    }
}