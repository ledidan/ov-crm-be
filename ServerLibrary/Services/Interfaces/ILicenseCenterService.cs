

using System.Security.Claims;
using Data.Applications;
using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services
{
    public interface ILicenseCenterService
    {
        // Application
        Task<List<Application>> GetApplicationsAsync();
        Task<DataObjectResponse> CreateApplicationAsync(CreateApplicationDTO app);

        Task<DataObjectResponse> UpdateApplicationAsync(ApplicationDTO app);

        // Application Plan
        Task<List<ApplicationPlanDTO>> GetPlansByApplicationIdAsync(int appId);
        Task<DataObjectResponse> CreatePlanAsync(ApplicationPlanDTO plan);

        Task<DataObjectResponse> UpdatePlanAsync(ApplicationPlanDTO plan);

        // Partner License
        Task<List<PartnerLicenseDTO>> GetPartnerLicensesAsync(int partnerId);
        Task<DataObjectResponse> CreateLicenseAsync(PartnerLicenseDTO license);
        Task<DataObjectResponse> RenewLicenseAsync(int licenseId, int durationDays);
        Task<bool> ExpireLicenseAsync(int licenseId);
        Task<bool> IsLicenseExpiredAsync(int partnerId, int applicationId);

        Task<DataObjectResponse> AssignDefaultApplicationLicenseToPartner(int partnerId);

        Task<bool> ValidLicenseAsync(int appId, ClaimsPrincipal user, List<AppClaim>? cachedApps = null);

        Task<List<UserLicenseDto>> GetUsersWithLicensesAsync(int partnerId);
        Task<UserLicenseDto> GetLicenseDetailsByUserAsync(int userId, int partnerId);

        Task<List<PartnerLicense>> GetOrCreateLicenses(AppPaymentRequest request);

        Task<DataObjectResponse> ActivateLicense(ActivateLicenseRequest request);


        Task SendActivationEmailAsync(string email, string fullName, string activationCode, string appName);
        Task<bool?> IsLicenseActiveAsync(int userId);
    }

}