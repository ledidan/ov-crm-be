using Data.DTOs;
using Data.DTOs.Contact;
using Data.Enums;
using Mapper.ContactMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        private readonly IPartnerService _partnerService;

        private readonly IEmployeeService _employeeService;
        public ContactController(IContactService contactService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _contactService = contactService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }
        [HttpPost("create-contact")]
        [Authorize(Roles = "User,Admin,SysAdmin")]
        public async Task<IActionResult> CreateContactAsync(CreateContact contact)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _contactService.CreateAsync(contact, employee, partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllContactAsync()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
                return BadRequest("Model is empty");
            try
            {
                var result = await _contactService.GetAllAsync(employee, partner);
                var resultDTO = result.Select(x => x.ToContactDTO()).ToList();
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetContactByIdAsync([FromRoute] int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (employee == null || partner == null)
            {
                return BadRequest("Invalid employeeId or partnerId provided.");
            }

            var contact = await _contactService.GetByIdAsync(id, employee, partner);

            if (contact == null)
            {
                return NotFound($"Contact with ID {id} not found for employeeId {employee.Id} and partnerId {partner.Id}.");
            }

            return Ok(contact.ToContactDTO());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateContactAsync([FromRoute] int id,
    [FromBody] UpdateContactDTO newUpdate
)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return NotFound($"Contact with ID {id} not found.");
            }

            var contact = await _contactService.UpdateContactIdAsync(id, newUpdate, employee, partner);

            return Ok(contact);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoveContactAsync([FromRoute] int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (employee == null)
            {
                return NotFound($"{employee.Id} not found");
            }

            var contactResult = await _contactService.DeleteIdAsync(id, employee, partner);
            return Ok(contactResult);
        }
        [HttpDelete("delete-bulk")]
        public async Task<IActionResult> DeleteMultipleContacts([FromQuery] string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var response = await _contactService.DeleteBulkContacts(ids, employee, partner);

            if (!response.Flag)
            {
                return response.Message.Contains("not found") ? NotFound(response) :
                       response.Message.Contains("authorized") ? Unauthorized(response) :
                       BadRequest(response);
            }

            return Ok(response);
        }

    }
}

