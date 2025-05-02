using AutoMapper;
using Data.Entities;
namespace Data.DTOs
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDTO>().ReverseMap();
            CreateMap<Customer, OptionalCustomerDTO>()
                .ForMember(dest => dest.CustomerEmployees, opt => opt.MapFrom(src =>
                    src.CustomerEmployees.Select(e => new CustomerEmployees
                    {
                        CustomerId = e.CustomerId,
                        EmployeeId = e.EmployeeId,
                    })));
            CreateMap<Customer, OptionalCustomerDTO>()
                .ForMember(dest => dest.CustomerContacts, opt => opt.MapFrom(src =>
                    src.CustomerContacts.Select(e => new CustomerContacts
                    {
                        CustomerId = e.CustomerId,
                        ContactId = e.ContactId
                    })));
            CreateMap<CustomerDTO, Customer>()
    .ForMember(dest => dest.PartnerId, opt => opt.Ignore());
            CreateMap<Customer, OptionalCustomerDTO>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<OptionalCustomerDTO, Customer>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

    }
}