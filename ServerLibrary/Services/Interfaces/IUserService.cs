using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IUserService
    {
        // ** START Handle Create Account Service System - Admin
        Task<GeneralResponse> HandleVerifiedUserWithoutPartnerAsync(ApplicationUser user, CreatePartner partner);
        Task<GeneralResponse> HandleUserWithActiveLicenseAsync(string email, CreatePartner partner);
        Task<GeneralResponse> HandleNewUserRegistrationAsync(RegisterAdmin user, CreatePartner partner);
         Task<DataObjectResponse> CheckActiveLicenseForRedirectAsync(ApplicationUser user);
        // ** END Handle Create Account Service System - Admin


        Task<GeneralResponse> CreateSysAdminAsync(RegisterSysAdmin user, string? role);
        Task<GeneralResponse> CreateUnverifiedAdminAsync(RegisterAdmin user, CreatePartner partner);
        Task<LoginResponse> SignInAppAsync(Login user);
        Task<LoginResponse> SignInGuestAsync(Login user);
        Task<LoginResponse> RefreshTokenAsync(RefreshToken token);


        // Task<GeneralResponse> SendRegisterTokenAsync(RegisterUserDTO user);
        Task<GeneralResponse> CreateUnverifiedUserByPartnerAsync(RegisterUserDTO user);
        Task<GeneralResponse> SendVerificationEmailAsync(ApplicationUser user);

        Task<GeneralResponse> SendVerificationEmailForGuestAsync(ApplicationUser user);
        Task<GeneralResponse> VerifyAsync(string email, string token);

        Task<GeneralResponse> SetPasswordAsync(SetPasswordDTO newUser);

        Task<ApplicationUser?> FindUserByEmail(string? email);
        Task<GeneralResponse> ResendVerificationAsync(string email);

        Task<List<ApplicationUser?>> GetAllMembersAsync(Partner partner);

        Task<GeneralResponse> ResetPasswordAsync(ResetPasswordDTO request);

        Task<string> GeneratePasswordResetTokenAsync(string? email, string? phoneNumber = null);

        Task<bool> IsValidResetTokenAsync(string? email, string? phoneNumber, string token);

        Task<GeneralResponse> RegisterForGuestAsync(RegisterGuestDTO guest);

        Task<ApplicationUser> GetApplicationUserByIdAsync(int id);


        // Task<DataObjectResponse> AssignDefaultApplicationsToPartner(int partnerId, PartnerLicenseDTO partnerLicense);

    };

}
