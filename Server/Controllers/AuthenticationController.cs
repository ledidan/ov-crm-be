using Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserService userService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> CreateAsync(Register user)
        {
            if (user == null) return BadRequest("User is empty");

            var result = await userService.CreateAsync(user);
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
