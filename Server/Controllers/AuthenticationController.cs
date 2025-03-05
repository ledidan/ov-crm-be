using System.Security.Claims;
using Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserService userService, IPartnerService partnerService) : ControllerBase
    {
        // [HttpPost("register-guest")]
        // [Authorize(Roles = "Admin,SysAdmin")]
        // public async Task<IActionResult> CreateUserAsync(Register user)
        // {
        //     if (user == null) return BadRequest("Thông tin người dùng không được để trống");

        //     var result = await userService.CreateUnverifiedAdminAsync(user);
        //     return Ok(result);
        // }

        [HttpPost("register-guest")]
        // [Authorize(Roles = "SysAdmin")]
        public async Task<IActionResult> CreateAdminAsync(RegisterAdmin user)
        {
            if (user == null) return BadRequest("Vui lòng nhập đầy đủ thông tin đăng ký");

            string role = Constants.Role.Admin;
            var result = await userService.CreateUnverifiedAdminAsync(user);
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
        public async Task<IActionResult> SigninAsync(Login user)
        {
            if (user == null) return BadRequest("User is empty");
            var result = await userService.SignInAsync(user);
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
            var result = await userService.CreateUnverifiedUserAsync(userDto);

            if (!result.Flag)
            {
                return BadRequest(new { Flag = result.Flag, message = result.Message });
            }

            return Ok(new { Flag = result.Flag, Message = result.Message });
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
    }
}
