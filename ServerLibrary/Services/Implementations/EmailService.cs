

using MimeKit;
using MailKit.Net.Smtp;
using RazorLight;
using System.IO;
using System.Threading.Tasks;
using Data.DTOs;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{

    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Ovie System Service", "didan.mobe@gmail.com"));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            email.ReplyTo.Add(new MailboxAddress("Ovie System Service", "didan.moeb@gmail.com"));


            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync("didan.mobe@gmail.com", "blhs alnu jdyw rfzd");
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);
        }

        public async Task<string> GetEmailTemplateAsync(string fullName, string verificationLink, string templateName)
        {
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName;
            string templateFolder = Path.Combine(projectRoot, "ServerLibrary", "Templates");
            string templatePath = Path.Combine(templateFolder, templateName);

            if (!Directory.Exists(templateFolder))
            {
                throw new DirectoryNotFoundException($"Templates folder not found: {templateFolder}");
            }
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }
            Console.WriteLine($"projectRoot {projectRoot}");
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templateFolder)
                .UseMemoryCachingProvider()
                .Build();
            var model = new EmailTemplateModel
            {
                FullName = fullName,
                VerificationLink = verificationLink
            };
            engine.Handler.Options.AdditionalMetadataReferences.Add(
    Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(EmailTemplateModel).Assembly.Location)
);
            string emailBody = await engine.CompileRenderAsync<EmailTemplateModel>(templateName, model);
            return emailBody;
        }

    }
}