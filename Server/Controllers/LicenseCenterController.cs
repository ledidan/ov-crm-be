
using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServerLibrary.Data;
using ServerLibrary.Services;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseCenterController : ControllerBase
    {
        private readonly AppDbContext _dbcontext;
        private readonly ILicenseCenterService _licenseService;

        private readonly IPartnerService _partnerService;

        public LicenseCenterController(
            IPartnerService partnerService,
            ILicenseCenterService licenseService,
            AppDbContext dbcontext
        )
        {
            _licenseService = licenseService;
            _partnerService = partnerService;
            _dbcontext = dbcontext;
        }

        // ──────── APPLICATIONS ────────

        [HttpGet("applications")]
        public async Task<IActionResult> GetApplications()
        {
            var apps = await _licenseService.GetApplicationsAsync();
            return Ok(apps);
        }

        [HttpPost("applications")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationDTO app)
        {
            var created = await _licenseService.CreateApplicationAsync(app);
            return CreatedAtAction(nameof(GetApplications), created);
        }

        // ──────── APPLICATION PLANS ────────

        [HttpGet("applications/{appId}/plans")]
        public async Task<IActionResult> GetPlans(int appId)
        {
            var plans = await _licenseService.GetPlansByApplicationIdAsync(appId);
            return Ok(plans);
        }

        [HttpPost("applications/plans")]
        public async Task<IActionResult> CreatePlan([FromBody] ApplicationPlanDTO plan)
        {
            var created = await _licenseService.CreatePlanAsync(plan);
            return CreatedAtAction(nameof(GetPlans), new { appId = plan.ApplicationId }, created);
        }

        // ──────── PARTNER LICENSES ────────

        [HttpGet("partners/licenses")]
        public async Task<IActionResult> GetPartnerLicenses()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if(partner == null)
            {
                return Unauthorized("Unauthorized access");
            }
            var licenses = await _licenseService.GetPartnerLicensesAsync(partner.Id);
            return Ok(licenses);
        }

        [HttpPost("partners/licenses")]
        public async Task<IActionResult> CreateLicense([FromBody] PartnerLicenseDTO license)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if (partner == null)
            {
                return Unauthorized("Unauthorized access");
            }
            license.PartnerId = partner.Id;
            var created = await _licenseService.CreateLicenseAsync(license);
            return CreatedAtAction(nameof(GetPartnerLicenses), new { partnerId = partner.Id }, created);
        }

        [HttpPost("licenses/{licenseId}/renew")]
        public async Task<IActionResult> RenewLicense(int licenseId, [FromBody] int durationDays)
        {
            var renewed = await _licenseService.RenewLicenseAsync(licenseId, durationDays);
            return Ok(renewed);
        }

        [HttpPost("licenses/{licenseId}/expire")]
        public async Task<IActionResult> ExpireLicense(int licenseId)
        {
            var success = await _licenseService.ExpireLicenseAsync(licenseId);
            return success ? Ok("Expired") : NotFound();
        }
        /// <summary>
        /// Lấy danh sách users và licenses của partner
        /// </summary>
        /// <param name="partnerId">ID của partner</param>
        /// <returns>Danh sách users với thông tin licenses</returns>
        [HttpGet("partners/user-licenses")]
        public async Task<IActionResult> GetUsersWithLicenses()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            // Validate PartnerId từ JWT
            var jwtPartnerId = User.FindFirst("PartnerId")?.Value;
            if (string.IsNullOrEmpty(jwtPartnerId) || int.Parse(jwtPartnerId) != partner.Id)
            {
                return Unauthorized("Invalid PartnerId or unauthorized access");
            }

            try
            {
                var userLicenses = await _licenseService.GetUsersWithLicensesAsync(partner.Id);
                if (!userLicenses.Any())
                {
                    return NotFound("No users or licenses found for this partner");
                }
                return Ok(userLicenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("data")]
        public async Task<IActionResult> GetData(int appId)
        {
            if (!await _licenseService.ValidLicenseAsync(appId, User))
                return Unauthorized("Invalid or expired license.");
            // Logic lấy data
            return Ok();
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckLicense([FromQuery] int UserId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if (UserId == null)
            {
                return Unauthorized("Unauthorized access");
            }
            if (partner == null)
            {
                return Unauthorized("Unauthorized access");
            }
            try
            {
                var userLicenses = await _licenseService.GetLicenseDetailsByUserAsync(UserId, partner.Id);
                if (userLicenses == null)
                {
                    return NotFound("No users or licenses found for this partner");
                }
                return Ok(userLicenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateLicenseRequest request)
        {
            try
            {
                var response = await _licenseService.ActivateLicense(request);
                
                if (response.Flag)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống, thử lại sau!, {ex.Message}");
            }
        }

        [HttpGet("is-active")]
        public async Task<IActionResult> CheckLicenseActive(int userId)
        {
            var isActive = await _licenseService.IsLicenseActiveAsync(userId);
            if (isActive == null)
            {
                return NotFound("License not found");
            }
            return Ok(isActive);
        }

        [HttpPut("application/{id}")]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody] ApplicationDTO app)
        {
            if (id != app.ApplicationId)
            {
                return BadRequest("Application ID mismatch");
            }
            var updated = await _licenseService.UpdateApplicationAsync(app);
            if (updated.Flag)
            {
                return Ok(updated);
            }
            return BadRequest(updated);
        }

        [HttpPut("plan/{id}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] ApplicationPlanDTO plan)
        {
            if (id != plan.Id)
            {
                return BadRequest("Application Plan ID mismatch");
            }
            var updated = await _licenseService.UpdatePlanAsync(plan);
            if (updated.Flag)
            {
                return Ok(updated);
            }
            return BadRequest(updated);
        }
    }
}