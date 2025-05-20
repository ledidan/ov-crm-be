using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ICRMService
    {
        Task<DataObjectResponse> FirstSetupCRMPartnerAsync(int partnerId, int userId, int employeeId);
    }
}