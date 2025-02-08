using AutoMapper;
using Data.Entities; 
namespace Data.DTOs
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, CustomerDTO>().ReverseMap();
        }
    }
}