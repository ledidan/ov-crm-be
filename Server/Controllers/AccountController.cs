



using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.MiddleWare;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    // [RequireValidLicense]
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IPartnerService _partnerService;

        public AccountController(IAccountService accountService, IPartnerService partnerService)
        {
            _accountService = accountService;
            _partnerService = partnerService;
        }
        [HttpGet("users")]
        public async Task<IActionResult> GetMergedEmployeeUserData()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var data = await _accountService.GetMergedEmployeeUserDataAsync(partner);
            return Ok(data);
        }
        
        [HttpGet("{id:int}/licenses")]
        public async Task<IActionResult> GetAllLicenses(int id) //** User ID
        {
            if(id == null) {
                return BadRequest("User ID is required.");
            }

            var data = await _accountService.GetAllLicensesAsync(id);

            return Ok(data);
        }
        [HttpGet("{id:int}/historyPayment")]
        public async Task<IActionResult> GetAllHistoryPayment(int id) //** User ID
        {
            if(id == null) {
                return BadRequest("User ID is required.");
            }

            var data = await _accountService.GetAllHistoryPaymentLicenseAsync(id);

            return Ok(data);
        }
    }
}