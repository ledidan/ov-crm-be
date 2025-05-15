

using Data.DTOs;

namespace ServerLibrary.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task<string> GetEmailTemplateAsync(string fullName, string verificationLink, string templateName);

        Task<string> GetResetPasswordTemplateAsync(ResetPasswordModel model, string templateName);

        Task<string> GetActivateEmailTemplateAsync(ActivationEmailModel model, string templateName);
    }
    
}