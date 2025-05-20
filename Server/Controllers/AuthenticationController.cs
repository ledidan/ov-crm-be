using System.Security.Claims;
using Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserService userService, IPartnerService partnerService) : ControllerBase
    {

        [HttpPost("register-admin")]

        public async Task<IActionResult> CreateAdminAsync([FromBody] RegisterAdminWithPartner request)
        {
            if (request.User == null || request.Partner == null) return BadRequest("Vui lòng nhập đầy đủ thông tin người dùng đăng ký");

            var result = await userService.CreateUnverifiedAdminAsync(request.User, request.Partner);
            return Ok(result);
        }

        // [HttpPost("register-sysadmin")]
        // // [Authorize(Roles = "SysAdmin")]
        // public async Task<IActionResult> CreateSysAdminAsync(Register user)
        // {
        //     if (user == null) return BadRequest("User is empty");

        //     string role = Constants.Role.SysAdmin;
        //     var result = await userService.CreateUnverifiedAdminAsync(user, role);
        //     return Ok(result);
        // }

        [HttpPost("login")]
        public async Task<IActionResult> SigninAppAsync(Login user)
        {
            if (user == null) return BadRequest("User is empty");
            var result = await userService.SignInAppAsync(user);
            return Ok(result);
        }

        [HttpPost("login-guest")]
        public async Task<IActionResult> SigninAsGuestAsync(Login user)
        {
            if (user == null) return BadRequest("User is empty");
            var result = await userService.SignInGuestAsync(user);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshToken token)
        {
            if (token == null) return BadRequest("Token is empty");
            var result = await userService.RefreshTokenAsync(token);
            return Ok(result);
        }

        [HttpPost("register-new-user")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTO userDto)
        {
            var result = await userService.CreateUnverifiedUserByPartnerAsync(userDto);

            return Ok(result);
        }

        [HttpPost("active-new-partner")]
        public async Task<IActionResult> RegisterNewPartnerForActiveLicense([FromBody] RegisterNewPartnerForActiveLicense request)
        {
            if (request.Email == null || request.createPartner == null)
                return BadRequest(new { message = "Email hoặc thông tin đối tác là bắt buộc" });
            var result = await userService.HandleUserWithActiveLicenseAsync(request.Email, request.createPartner);

            if (!result.Flag)
            {
                return BadRequest(new { Flag = result.Flag, message = result.Message });
            }

            return Ok(new { Flag = result.Flag, Message = result.Message });
        }

        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.PhoneNumber))
                return BadRequest(new { Flag = false, Message = "Email hoặc số điện thoại là bắt buộc" });

            string token = await userService.GeneratePasswordResetTokenAsync(request.Email, request.PhoneNumber);
            if (token == null)
                return NotFound(new { Flag = false, Message = "Không tìm thấy tài khoản" });

            // TODO: Gửi email hoặc SMS chứa token (chưa tích hợp)
            return Ok(new { Flag = true, Message = $"Mã kích hoạt đã được gửi tới email: {request.Email}" });
        }

        [HttpPost("reset-new-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO request)
        {
            var result = await userService.ResetPasswordAsync(request);
            if (!result.Flag)
                return BadRequest(result);

            return Ok(new { Flag = true, Message = "Mật khẩu đã được cập nhật thành công" });
        }

        [HttpGet("activate-user")]
        public async Task<IActionResult> Verify([FromQuery] string email, [FromQuery] string token)
        {
            var response = await userService.VerifyAsync(email, token);
            if (response.Flag)
            {
                // var userId = (response.Data as dynamic)?.UserId;
                // return Redirect($"https://yourapp.com/set-password?userId={userId}");
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromBody] ValidateTokenDto request)
        {
            bool isValid = await userService.IsValidResetTokenAsync(request.Email, request.PhoneNumber, request.Token);
            if (!isValid)
                return BadRequest(new { Flag = false, Message = "Mã code không hợp lệ hoặc đã sử dụng" });

            return Ok(new { Flag = true, Message = "Mã code hợp lệ" });
        }

        [HttpPost("send-email-verification")]
        public async Task<IActionResult> SendVerificationEmail([FromBody] EmailRequest request)
        {
            var user = await userService.FindUserByEmail(request.Email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var response = await userService.SendVerificationEmailAsync(user);
            if (!response.Flag)
            {
                return BadRequest(new { Flag = response.Flag, message = response.Message });
            }

            return Ok(new { Flag = response.Flag, message = response.Message });
        }

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDTO newUser)
        {
            var response = await userService.SetPasswordAsync(newUser);
            if (response.Flag) return Ok(response);
            return BadRequest(response);
        }


        [HttpPost("resend-email-verification")]
        public async Task<IActionResult> ResendEmail([FromBody] EmailRequest request)
        {
            var response = await userService.ResendVerificationAsync(request.Email);
            if (response.Flag) return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("Member/get-all")]
        public async Task<IActionResult> GetAllMembersAsync()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);


            var result = await userService.GetAllMembersAsync(partner);

            return Ok(result);
        }
        [HttpPost("register-guest")]
        public async Task<IActionResult> CreateGuest([FromBody] RegisterGuestDTO userDto)
        {
            var result = await userService.RegisterForGuestAsync(userDto);

            if (!result.Flag)
            {
                return BadRequest(new { Flag = result.Flag, Message = result.Message });
            }

            return Ok(new { Flag = result.Flag, Message = result.Message });
        }
    }
}
