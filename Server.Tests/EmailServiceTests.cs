

using ServerLibrary.Services.Interfaces;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using ServerLibrary.Services.Implementations;
using RazorLight;
using Data.DTOs;
namespace Server.Tests

{
    public class EmailServiceTests
    {
        [Fact]
        public async Task GetEmailTemplateAsync_UsesRealTemplate_ReturnsExpectedHtml()
        {
            // Arrange
            var mockEnv = new Mock<IWebHostEnvironment>();

            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory));
            string templatesPath = Path.Combine(projectRoot, "Templates");

            Console.WriteLine("ContentRootPath: " + projectRoot);
            Console.WriteLine("Template folder: " + templatesPath);

            var templateName = "EmailVerificationTemplate.cshtml";
            var templatePath = Path.Combine(templatesPath, templateName);

            Assert.True(File.Exists(templatePath), $"Expected template not found at: {templatePath}");

            mockEnv.Setup(e => e.ContentRootPath).Returns(projectRoot);

            var service = new EmailService(mockEnv.Object);

            var fullName = "John Doe";
            var verificationLink = "https://example.com/verify";

            // Act
            var result = await service.GetEmailTemplateAsync(fullName, verificationLink, templateName);

            // Assert
            Assert.Contains(fullName, result);
            Assert.Contains(verificationLink, result);
        }

    }
}

