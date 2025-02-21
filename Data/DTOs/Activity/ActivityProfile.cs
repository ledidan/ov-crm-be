using AutoMapper;
using Data.Entities; 
namespace Data.DTOs
{
    public class ActivityProfile : Profile
    {
        public ActivityProfile()
        {
            CreateMap<Activity, ActivityDTO>();
        }
    }
}