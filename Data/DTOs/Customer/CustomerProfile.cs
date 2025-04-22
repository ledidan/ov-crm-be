using AutoMapper;
using Data.Entities;
namespace Data.DTOs
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDTO>().ReverseMap();

            CreateMap<CustomerDTO, Customer>()
    .ForMember(dest => dest.PartnerId, opt => opt.Ignore());
            CreateMap<Customer, OptionalCustomerDTO>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<OptionalCustomerDTO, Customer>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

    }
}