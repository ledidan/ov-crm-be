using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.MongoModels;

public class QuoteProfile : Profile
{
    public QuoteProfile()
    {
        CreateMap<QuoteDTO, Order>(); // Maps QuoteDTO to Quote entity
        CreateMap<Quote, OptionalQuoteDTO>();
        CreateMap<CreateQuoteDTO, Quote>();
        CreateMap<OptionalQuoteDTO, Quote>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<UpdateQuoteDTO, Quote>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<QuoteDTO, Quote>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Quote, QuoteDTO>();
        CreateMap<QuoteDetailsDTO, QuoteDetails>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore ID if it's managed by MongoDB
            .ReverseMap();
    }
}
