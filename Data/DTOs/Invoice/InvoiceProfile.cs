using AutoMapper;
using Data.Entities;

namespace Data.DTOs
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            CreateMap<Invoice, InvoiceDTO>();
            CreateMap<Invoice, ContactInvoiceDTO>();
            CreateMap<ContactInvoiceDTO, Invoice>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<InvoiceDTO, Invoice>();
            CreateMap<InvoiceDTO, Invoice>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
