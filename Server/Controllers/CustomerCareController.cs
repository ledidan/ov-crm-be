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

    public class CustomerCareController : ControllerBase
    {
        private readonly ICustomerCareService _customerCareService;
        private readonly IPartnerService _partnerService;
        private readonly IEmployeeService _employeeService;

        public CustomerCareController(ICustomerCareService customerCareService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _customerCareService = customerCareService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }

        [HttpGet("customer-care-tickets")]
        public async Task<IActionResult> GetAllCustomerCareTickets()
        {
            var customerCareTickets = await _customerCareService.GetAllCustomerCareTickets();
            return Ok(customerCareTickets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerCareTicketById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var customerCareTicket = await _customerCareService.GetCustomerCareTicketById(id, partner);
            return Ok(customerCareTicket);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCustomerCareTicket([FromBody] CustomerCareTicketDTO customerCareTicketDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var customerCareTicket = await _customerCareService.CreateCustomerCareTicket(customerCareTicketDTO, employee, partner);
            if (!customerCareTicket.Flag)
            {
                return BadRequest(customerCareTicket.Message);
            }
            return Ok(customerCareTicket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomerCareTicket(int id, [FromBody] CustomerCareTicketDTO customerCareTicketDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var customerCareTicket = await _customerCareService.UpdateCustomerCareTicket(id, customerCareTicketDTO, employee, partner);
            if (!customerCareTicket.Flag)
            {
                return BadRequest(customerCareTicket.Message);
            }
            return Ok(customerCareTicket);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerCareTicket(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var customerCareTicket = await _customerCareService.DeleteCustomerCareTicket(id, partner);
            return Ok(customerCareTicket);
        }

        [HttpGet("{id}/activities")]
        public async Task<IActionResult> GetAllActivitiesByCustomerCareTickets(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var activities = await _customerCareService.GetAllActivitiesByCustomerCareTickets(id, partner);
            return Ok(activities);
        }

        [HttpGet("{id}/activities/done")]
        public async Task<IActionResult> GetAllActivitiesDoneByCustomerCareTickets(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var activities = await _customerCareService.GetAllActivitiesDoneByCustomerCareTickets(id, partner);
            return Ok(activities);
        }

        [HttpPut("{id}/activities/unassign")]
        public async Task<IActionResult> UnassignActivityFromId(int id, [FromBody] int activityId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            var partner = await _partnerService.FindByClaim(identity);

            var result = await _customerCareService.UnassignActivityFromId(id, activityId, partner);

            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result); 
        }


        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteBulkCustomerTicketsAsync(string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _customerCareService.DeleteBulkCustomerTicketsAsync(ids, employee, partner);
            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }


        [HttpPost("check-code")]
        public async Task<IActionResult> CheckCustomerTicketCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _customerCareService.CheckCustomerCareCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateCustomerTicketCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _customerCareService.GenerateCustomerCareCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}