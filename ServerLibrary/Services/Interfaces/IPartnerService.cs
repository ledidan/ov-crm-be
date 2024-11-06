using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IPartnerService
    {
        Task<GeneralResponse> CreateAsync(CreatePartner partner);
        Task<Partner?> FindById(int id);
        Task<List<Partner>> GetAsync();
    }
}
