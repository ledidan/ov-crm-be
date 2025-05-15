using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ICRMService
    {
        Task<EmployeeDTO> SeedDefaultEmployeeRolesAndAdminAsync(int partnerId, ApplicationUser adminUser);

        Task<DataObjectResponse> FirstSetupCRMPartnerAsync(CreatePartner partner, int userId);
    }
}