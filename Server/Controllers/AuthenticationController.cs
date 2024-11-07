using Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserService userService) : ControllerBase
    {
        [HttpPost("register-user")]
        [Authorize(Roles = "Admin,SysAdmin")]
        public async Task<IActionResult> CreateUserAsync(Register user)
        {
            if (user == null) return BadRequest("User is empty");

            string role = Constants.Role.User;
            var result = await userService.CreateAsync(user, role);
            return Ok(result);
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = "SysAdmin")]
        public async Task<IActionResult> CreateAdminAsync(Register user)
        {
            if (user == null) return BadRequest("User is empty");

            string role = Constants.Role.Admin;
            var result = await userService.CreateAsync(user, role);
            return Ok(result);
        }

        [HttpPost("register-sysadmin")]
        [Authorize(Roles = "SysAdmin")]
        public async Task<IActionResult> CreateSysAdminAsync(Register user)
        {
            if (user == null) return BadRequest("User is empty");

            string role = Constants.Role.SysAdmin;
            var result = await userService.CreateAsync(user, role);
            return Ok(result);
        }

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
    }
}
