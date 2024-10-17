using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IPartnerService
    {
        Task<GeneralResponse> CreateAsync(CreatePartner user);
        Task<Partner?> FindPartnerById(int id);
    }
}
