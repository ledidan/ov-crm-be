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
    public class PartnerController(IPartnerService partnerService,
    IUserService userService,
    ICRMService crmService,
    IEmployeeService employeeService) : Controller
    {

        [HttpPost("setup")]
        public async Task<IActionResult> CreatePartnerAsync(CreatePartner partner)
        {
            if (partner == null) return BadRequest("User is empty");

            var result = await partnerService.CreatePartnerFreeTrialAsync(partner);
            return Ok(result);
        }
        [HttpGet("partner")]
        public async Task<IActionResult> GetPartnerById()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng từ claims");
            }

            var partner = await partnerService.FindByClaim(identity);
            if (partner == null)
            {
                return NotFound("Không tìm thấy thông tin doanh nghiệp");
            }

            var result = await partnerService.GetPartnerInfoAsync(partner.Id);
            if (result == null)
            {
                return NotFound("Không tìm thấy thông tin doanh nghiệp với ID này");
            }

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("initialize")]
        public async Task<IActionResult> InitializePartnerAsync([FromBody] RequestInitializePartner request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner.Id < 0 || request.UserId < 0 || employee.Id < 0)
            {
                return NotFound("Partner not found or user not found");
            }
            var IsUserOwner = await partnerService.CheckClaimByOwner(identity);
            if (IsUserOwner != null && IsUserOwner == false)
                return Forbid("Bạn không phải là Owner để khởi tạo CRM.");

            var result = await crmService.FirstSetupCRMPartnerAsync(partner.Id, request.UserId, employee.Id);
            if (result == null)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
