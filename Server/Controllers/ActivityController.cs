using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
using Mapper.ContactMapper;
using Mapper.CustomerMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;
        private readonly IPartnerService _partnerService;

        private readonly IEmployeeService _employeeService;
        public ActivityController(IActivityService activityService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _activityService = activityService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }


        [HttpGet("get-all")]

        public async Task<IActionResult> GetAllActivities()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
                return BadRequest("Model is empty");
            try
            {
                var result = await _activityService.GetAllActivityAsync(employee, partner);
                var resultDTO = result.ToList();
                return Ok(resultDTO);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred. Please try again later." });
            }
        }
    }
}
