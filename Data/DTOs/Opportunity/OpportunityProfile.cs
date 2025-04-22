using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.MongoModels;

public class OpportunityProfile : Profile
{
    public OpportunityProfile()
    {
        CreateMap<OpportunityDTO, Opportunity>(); // Maps QuoteDTO to Quote entity
        CreateMap<Opportunity, OptionalOpportunityDTO>();
        CreateMap<CreateOpportunityDTO, Opportunity>();
        CreateMap<OptionalOpportunityDTO, Opportunity>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<UpdateOpportunityDTO, Opportunity>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<OpportunityDTO, Opportunity>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Opportunity, OpportunityDTO>();
        CreateMap<OpportunityProductDetailsDTO, OpportunityProductDetails>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Ignore ID if it's managed by MongoDB
            .ReverseMap();
    }
}
