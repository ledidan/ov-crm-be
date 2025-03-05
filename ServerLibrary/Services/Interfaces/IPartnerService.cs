using Data.DTOs;
using Data.Entities;
using Data.Responses;
using System.Security.Claims;

namespace ServerLibrary.Services.Interfaces
{
    public interface IPartnerService
    {
        Task<DataObjectResponse> CreateAsync(CreatePartner partner);
        Task<Partner?> FindById(int id);
        Task<List<Partner>> GetAsync();
        Task<Partner?> FindByClaim(ClaimsIdentity? claimsIdentity);
        Task<bool?> CheckClaimByOwner(ClaimsIdentity? claimsIdentity);
    }
}
