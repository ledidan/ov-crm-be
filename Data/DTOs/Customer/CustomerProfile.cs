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

            CreateMap<CreateCustomer, Customer>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.AccountName))
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.AccountNumber))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic))
                .ForMember(dest => dest.IsPartner, opt => opt.MapFrom(src => src.IsPartner))
                .ForMember(dest => dest.IsPersonal, opt => opt.MapFrom(src => src.IsPersonal))
                .ForMember(dest => dest.IsOldCustomer, opt => opt.MapFrom(src => src.IsOldCustomer))
                .ForMember(dest => dest.IsDistributor, opt => opt.MapFrom(src => src.IsDistributor))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId));

            CreateMap<Customer, CreateCustomer>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.AccountName))
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.AccountNumber))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic))
                .ForMember(dest => dest.IsPartner, opt => opt.MapFrom(src => src.IsPartner))
                .ForMember(dest => dest.IsPersonal, opt => opt.MapFrom(src => src.IsPersonal))
                .ForMember(dest => dest.IsOldCustomer, opt => opt.MapFrom(src => src.IsOldCustomer))
                .ForMember(dest => dest.IsDistributor, opt => opt.MapFrom(src => src.IsDistributor))
                .ForMember(dest => dest.PartnerId, opt => opt.MapFrom(src => src.PartnerId));
        }

    }
}