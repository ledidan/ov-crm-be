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
    public class SysAdminController(IUserService userService, IPartnerService partnerService) : ControllerBase
    {

        [HttpPost("create-sysadmin")]
        public async Task<IActionResult> CreateSysAdminAsync(RegisterSysAdmin user)
        {
            if (user == null) return BadRequest("User is empty");

            string role = Constants.Role.SysAdmin;
            var result = await userService.CreateSysAdminAsync(user, role);
            return Ok(result);
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = "SysAdmin")]
        public async Task<IActionResult> CreateAdminAsync(RegisterSysAdmin user)
        {
            string role = Constants.Role.Admin;
            var result = await userService.CreateSysAdminAsync(user, role);

            if (!result.Flag)
            {
                return BadRequest("Cannot create by user service create admin sys");
            }
            return Ok(result);
        }
    }
}