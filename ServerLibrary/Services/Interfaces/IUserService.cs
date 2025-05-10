using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IUserService
    {
        Task<GeneralResponse> CreateSysAdminAsync(RegisterSysAdmin user, string? role);
        Task<GeneralResponse> CreateUnverifiedAdminAsync(RegisterAdmin user);
        Task<LoginResponse> SignInAsync(Login user);
        Task<LoginResponse> RefreshTokenAsync(RefreshToken token);


        // Task<GeneralResponse> SendRegisterTokenAsync(RegisterUserDTO user);
        Task<GeneralResponse> CreateUnverifiedUserAsync(RegisterUserDTO user);
        Task<GeneralResponse> SendVerificationEmailAsync(ApplicationUser user);

        Task<GeneralResponse> VerifyAsync(string email, string token);

        Task<GeneralResponse> SetPasswordAsync(SetPasswordDTO newUser);

        Task<ApplicationUser?> FindUserByEmail(string? email);
        Task<GeneralResponse> ResendVerificationAsync(string email);

        Task<List<ApplicationUser?>> GetAllMembersAsync(Partner partner);

        Task<bool> ResetPasswordAsync(ResetPasswordDTO request);

        Task<string> GeneratePasswordResetTokenAsync(string? email, string? phoneNumber = null);

        Task<bool> IsValidResetTokenAsync(string? email, string? phoneNumber, string token);

        // Task<DataObjectResponse> AssignDefaultApplicationsToPartner(int partnerId, PartnerLicenseDTO partnerLicense);

    };

}
