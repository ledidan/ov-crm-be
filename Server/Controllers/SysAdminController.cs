using Data.DTOs;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SysAdmin")]
    public class SysAdminController(IPartnerService partnerService) : Controller
    {
        [HttpPost("create-partner")]
        public async Task<IActionResult> CreatePartnerAsync(CreatePartner partner)
        {
            if (partner == null) return BadRequest("User is empty");

            var result = await partnerService.CreateAsync(partner);
            return Ok(result);
        }
    }
}
