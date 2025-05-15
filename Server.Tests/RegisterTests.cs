
using ServerLibrary.Services.Interfaces;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using ServerLibrary.Services.Implementations;
using RazorLight;
using Data.DTOs;
namespace Server.Tests

{
    public class RegisterTests
    {
        private readonly IUserService _userService;
        [Fact]
        public async Task HandleNewUserRegistrationAsync_ValidInput_CreatesUserAndPartner()
        {
            var user = new RegisterAdmin { Email = "test@example.com", FullName = "Test User" };
            var partner = new CreatePartner { Name = "Test Partner", EmailContact = "test@example.com", ShortName = "123" };
            var result = await _userService.HandleNewUserRegistrationAsync(user, partner);
            Assert.True(result.Flag);
            Assert.Equal("Đăng ký thành công, vui lòng kiểm tra email để đặt mật khẩu!", result.Message);
        }
    }
}
