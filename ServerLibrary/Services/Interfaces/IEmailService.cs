

namespace ServerLibrary.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task<string> GetEmailTemplateAsync(string fullName, string verificationLink);
    }
    
}