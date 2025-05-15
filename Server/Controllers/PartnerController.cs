using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.MiddleWare;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "SysAdmin")]
    public class PartnerController(IPartnerService partnerService, ICRMService crmService) : Controller
    {

        [HttpPost("setup")]
        public async Task<IActionResult> CreatePartnerAsync(CreatePartner partner)
        {
            if (partner == null) return BadRequest("User is empty");

            var result = await partnerService.CreatePartnerFreeTrialAsync(partner);
            return Ok(result);
        }
        [HttpPost("CRM/first-setup")]
        public async Task<IActionResult> FirstSetupCRMPartner([FromBody] CreatePartner partner)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return BadRequest("User ID not found.");
                }

                var result = await crmService.FirstSetupCRMPartnerAsync(partner, int.Parse(userId));
                if (result.Flag)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }

}
