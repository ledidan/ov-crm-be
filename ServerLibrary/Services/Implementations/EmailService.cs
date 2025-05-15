using System.IO;
using System.Threading.Tasks;
using Data.DTOs;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RazorLight;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;
        public EmailService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Autuna System Service", "didan.mobe@gmail.com"));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            email.ReplyTo.Add(new MailboxAddress("Autuna System Service", "didan.mobe@gmail.com"));

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(
                "smtp.gmail.com",
                587,
                MailKit.Security.SecureSocketOptions.StartTls
            );
            await smtpClient.AuthenticateAsync("didan.mobe@gmail.com", "blhs alnu jdyw rfzd");
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);
        }

        public async Task<string> GetEmailTemplateAsync(
            string fullName,
            string verificationLink,
            string templateName
        )
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string templateFolder = Path.Combine(basePath, "Templates");
            string templatePath = Path.Combine(templateFolder, templateName);
            Console.WriteLine($"Template Folder: {templateFolder}");
            Console.WriteLine($"Template Path: {templatePath}");

            if (!Directory.Exists(templateFolder))
            {
                throw new DirectoryNotFoundException(
                    $"Templates folder not found: {templateFolder}"
                );
            }
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templateFolder)
                .UseMemoryCachingProvider()
                .Build();
            var model = new EmailTemplateModel
            {
                FullName = fullName,
                VerificationLink = verificationLink,
            };
            engine.Handler.Options.AdditionalMetadataReferences.Add(
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(
                    typeof(EmailTemplateModel).Assembly.Location
                )
            );
            string emailBody = await engine.CompileRenderAsync<EmailTemplateModel>(
                templateName,
                model
            );
            return emailBody;
        }


        public async Task<string> GetActivateEmailTemplateAsync(ActivationEmailModel model, string templateName)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string templateFolder = Path.Combine(basePath, "Templates");
            string templatePath = Path.Combine(templateFolder, templateName);       
            
            if (!Directory.Exists(templateFolder))
            {
                throw new DirectoryNotFoundException($"Templates folder not found: {templateFolder}");
            }
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }
            var engine = new RazorLightEngineBuilder()
                           .UseFileSystemProject(templateFolder)
                           .UseMemoryCachingProvider()
                           .Build();
            engine.Handler.Options.AdditionalMetadataReferences.Add(
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(
                    typeof(ActivationEmailModel).Assembly.Location
                )
            );
            string emailBody = await engine.CompileRenderAsync<ActivationEmailModel>(templateName, model);
            return emailBody;
        }


        public async Task<string> GetResetPasswordTemplateAsync(ResetPasswordModel model, string templateName)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string templateFolder = Path.Combine(basePath, "Templates");
            string templatePath = Path.Combine(templateFolder, templateName);       
            
            if (!Directory.Exists(templateFolder))
            {
                throw new DirectoryNotFoundException($"Templates folder not found: {templateFolder}");
            }
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }
            var engine = new RazorLightEngineBuilder()
                           .UseFileSystemProject(templateFolder)
                           .UseMemoryCachingProvider()
                           .Build();
            engine.Handler.Options.AdditionalMetadataReferences.Add(
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(
                    typeof(ResetPasswordModel).Assembly.Location
                )
            );
            string emailBody = await engine.CompileRenderAsync<ResetPasswordModel>(templateName, model);
            return emailBody;
        }
    }
}
