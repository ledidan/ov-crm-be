using AutoMapper;
using Data.Entities;
using Data.MongoModels;

namespace Data.DTOs
{
    public class InvoiceProfile : Profile
    {
        public InvoiceProfile()
        {
            CreateMap<Invoice, InvoiceDTO>();
            CreateMap<Invoice, ContactInvoiceDTO>();
            CreateMap<Invoice, UpdateInvoiceDTO>();
            CreateMap<ContactInvoiceDTO, Invoice>()

                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<InvoiceDTO, Invoice>()
                .ForMember(dest => dest.InvoiceOrders, opt => opt.Ignore()) // Ignore InvoiceOrders
                .ForMember(dest => dest.Orders, opt => opt.Ignore()) // Ignore Orders if it exists
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<InvoiceDetails, InvoiceDetailDTO>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<InvoiceDetailDTO, InvoiceDetails>()
                            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateInvoiceDTO, InvoiceDTO>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UpdateInvoiceDTO, InvoiceDTO>()
           .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Order, OrderInvoiceDTO>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
