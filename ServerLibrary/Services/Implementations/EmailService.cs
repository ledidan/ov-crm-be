using System.IO;
using System.Threading.Tasks;
using Data.DTOs;
using MailKit.Net.Smtp;
using MimeKit;
using RazorLight;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class EmailService : IEmailService
    {
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
            // string projectRoot = "/app";
            // if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            // {
            //     projectRoot = "/app";
            // }
            // else
            // {
            //     projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName;
            // }
            string basePath = AppContext.BaseDirectory;
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName; 
            string templateFolder = Path.Combine(basePath, "ServerLibrary", "Templates");
            string templatePath = Path.Combine(templateFolder, templateName);
            Console.WriteLine($"Dotnet running in container", Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));
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
            Console.WriteLine($"projectRoot {projectRoot}");
            Console.WriteLine($"basePath {basePath}");
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
    }
}
