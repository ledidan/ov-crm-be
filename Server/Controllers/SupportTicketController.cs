using Data.DTOs;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class SupportTicketController : ControllerBase
    {
        private readonly ISupportTicketService _supportTicketService;
        private readonly IPartnerService _partnerService;
        private readonly IEmployeeService _employeeService;
        public SupportTicketController(ISupportTicketService supportTicketService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _supportTicketService = supportTicketService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }
        [HttpGet("support-tickets")]
        public async Task<IActionResult> GetAllSupportTickets()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var supportTickets = await _supportTicketService.GetAllSupportTickets(partner);
            return Ok(supportTickets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupportTicketById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var supportTicket = await _supportTicketService.GetSupportTicketById(id, partner);
            return Ok(supportTicket);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSupportTicket([FromBody] SupportTicketDTO supportTicketDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var supportTicket = await _supportTicketService.CreateSupportTicket(supportTicketDTO, employee, partner);
            return Ok(supportTicket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupportTicket(int id, SupportTicketDTO supportTicketDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var supportTicket = await _supportTicketService.UpdateSupportTicket(id, supportTicketDTO, employee, partner);
            return Ok(supportTicket);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupportTicket(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var supportTicket = await _supportTicketService.DeleteSupportTicket(id, partner);
            return Ok(supportTicket);
        }

        [HttpGet("{id}/activities")]
        public async Task<IActionResult> GetAllActivitiesBySupportTickets(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var activities = await _supportTicketService.GetAllActivitiesBySupportTickets(id, partner);
            return Ok(activities);
        }

        [HttpGet("{id}/activities/done")]
        public async Task<IActionResult> GetAllActivitiesDoneBySupportTickets(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var activities = await _supportTicketService.GetAllActivitiesDoneBySupportTickets(id, partner);
            return Ok(activities);
        }

        [HttpPut("{id}/activities/unassign")]
        public async Task<IActionResult> UnassignActivityFromId(int id, [FromBody] int activityId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            var partner = await _partnerService.FindByClaim(identity);

            var result = await _supportTicketService.UnassignActivityFromId(id, activityId, partner);

            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteBulkTicketsAsync(string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _supportTicketService.DeleteBulkTicketsAsync(ids, employee, partner);
            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

          [HttpPost("check-code")]
        public async Task<IActionResult> CheckSupportCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _supportTicketService.CheckSupportCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateSupportCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _supportTicketService.GenerateSupportCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }

}