using Data.DTOs;
using Data.Entities;
using Data.Responses;
using System.Security.Claims;

namespace ServerLibrary.Services.Interfaces
{
    public interface IPartnerService
    {
        Task<DataObjectResponse> CreatePartnerFreeTrialAsync(CreatePartner partner);
        Task<Partner> CreatePartnerAsync(CreatePartner partner);
        Task<PartnerDTO> GetPartnerInfoAsync(int partnerId);
        Task<bool?> FindUserOfPartner(int userId);

        // Task<DataObjectResponse> CreateAndUpdatePartnerAfterActivatedLicenseAsync(UpdatePartner partner);
        Task<Partner?> FindById(int id);
        Task<List<Partner>> GetAsync();
        Task<Partner?> FindByClaim(ClaimsIdentity? claimsIdentity);
        Task<bool?> CheckClaimByOwner(ClaimsIdentity? claimsIdentity);

        Task<Partner> UpdatePartnerAsync(PartnerDTO model);
    }
}
