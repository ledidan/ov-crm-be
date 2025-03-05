

using ServerLibrary.Services.Interfaces;
using Xunit;
using Moq;
namespace Server.Tests

{
    public class EmailServiceTests
    {
        private readonly Mock<IEmailService> _emailServiceMock;

        public EmailServiceTests()
        {
            _emailServiceMock = new Mock<IEmailService>();
        }
        [Fact]
        public async Task GetEmailTemplateAsync_ShouldReturn_ValidHtml()
        {
            // Arrange
            var fullName = "Nguyen Van A";
            var verificationLink = "https://example.com/verify";
            // var expectedHtml = $"<h2>Xác thực Email</h2><p>{fullName}</p><a href='{verificationLink}'>Verify</a>";
            var expectedHtml = $"<p>Xin chào @Model.FullName,</p>";
            // Act
            _emailServiceMock
                .Setup(service => service.GetEmailTemplateAsync(fullName, verificationLink))
                .ReturnsAsync(expectedHtml);

            var emailService = _emailServiceMock.Object;

            // Act
            var emailBody = await emailService.GetEmailTemplateAsync(fullName, verificationLink);

            // Assert
            Assert.Contains(fullName, emailBody);
            Assert.Contains(verificationLink, emailBody);
            Assert.Contains("<h2>Xác thực Email</h2>", emailBody);
        }
    }
}

