using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IAccountService
    {

        Task<bool?> FindByClaim(ClaimsIdentity? claimsIdentity);
        Task<List<MergedEmployeeUserDTO>> GetMergedEmployeeUserDataAsync(Partner partner);
        Task<GeneralResponse> DeactivateAccount(int userId, Partner partner);

        Task<List<TransactionForUserDTO>> GetAllHistoryPaymentLicenseAsync(int userId);

        Task<List<LicenseForUserDTO>> GetAllLicensesAsync(int userId);

    }
}