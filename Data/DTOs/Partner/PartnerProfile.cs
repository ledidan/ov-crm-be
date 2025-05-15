using AutoMapper;
using Data.Entities;
using Data.MongoModels;

namespace Data.DTOs
{
    public class PartnerProfile : Profile
    {
        public PartnerProfile()
        {
            CreateMap<Partner, PartnerDTO>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}