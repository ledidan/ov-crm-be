using Data.DTOs;
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
        [HttpPost("create")]
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
        [Authorize]
        [HttpGet("contacts")]
        public async Task<IActionResult> GetAllContactAsync([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
                return BadRequest("Model is empty");
            try
            {
                var result = await _contactService.GetAllAsync(employee, partner, pageNumber, pageSize);
                // var resultDTO = result.Select(x => x.ToContactDTO()).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau." });
            }
        }
        [Authorize]
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
        [Authorize]
        [HttpPatch("{id:int}")]

        public async Task<IActionResult> UpdateFieldContact(int id,
                [FromBody] UpdateContactDTO updateContact)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (updateContact == null)
            {
                return BadRequest(new { message = "Invalid request data" });
            }

            if (employee == null || partner == null)
            {
                return Unauthorized(new { message = "Unauthorized access" });
            }

            var result = await _contactService.UpdateFieldIdAsync(id, updateContact,
             employee, partner);

            if (!result.Flag)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { Flag = result.Flag, Message = result.Message });

        }
        [Authorize]
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
            if (!contact.Flag)
            {
                return BadRequest(contact.Message);
            }
            return Ok(contact);
        }
        [Authorize]
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
        [Authorize]
        [HttpDelete("bulk-delete")]
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
        [Authorize]
        [HttpGet("{id:int}/orders")]
        public async Task<IActionResult> GetAllOrdersByContact(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (id == null)
            {
                return BadRequest("Cannot found orders id");
            }

            var result = await _contactService.GetAllOrdersByContactAsync(id, employee, partner);

            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest("Failed to get order");
        }
        [Authorize]
        [HttpGet("{id:int}/invoices")]
        public async Task<IActionResult> GetAllInvoicesByContact(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _contactService.GetAllInvoicesByContactAsync(id, employee, partner);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest("Lỗi khi lấy dữ liệu hoá đơn");
        }


        [HttpPut("{id:int}/orders/assign")]
        public async Task<IActionResult> AssignContactToOrder(int id, [FromBody] AssignOrderRequest request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (employee == null || partner == null)
            {
                return BadRequest("Invalid employeeId or partnerId provided.");
            }
            var result = await _contactService.AssignContactToOrderAsync(id, request, employee, partner);
            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [HttpPut("{id:int}/orders/unassign")]
        public async Task<IActionResult> UnassignContactToOrder(int id, [FromBody] AssignOrderRequest request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (employee == null || partner == null)
            {
                return BadRequest("Invalid employeeId or partnerId provided.");
            }
            var result = await _contactService.UnassignContactToOrderAsync(id, request, employee, partner);
            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [HttpPut("{id:int}/invoice/unassign")]
        public async Task<IActionResult> UnassignInvoiceFromContact(int id, [FromBody] int invoiceId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (employee == null || partner == null)
            {
                return BadRequest("Invalid employeeId or partnerId provided.");
            }
            var result = await _contactService.UnassignInvoiceFromContactAsync(id, invoiceId, employee, partner);
            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        [HttpGet("{id:int}/activities")]
        public async Task<IActionResult> GetAllActivitiesByContact(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (employee == null || partner == null)
            {
                return BadRequest("Invalid employeeId or partnerId provided.");
            }
            var result = await _contactService.GetAllActivitiesByContactAsync(id, employee, partner);
            return Ok(result);
        }

        [HttpPut("{id:int}/activity/unassign")]
        public async Task<IActionResult> UnassignActivityFromContact(int id, [FromBody] int activityId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (employee == null || partner == null)
            {
                return BadRequest("Invalid employeeId or partnerId provided.");
            }
            var result = await _contactService.UnassignActivityFromContactAsync(id, activityId, employee, partner);
            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("check-code")]
        public async Task<IActionResult> CheckContactCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _contactService.CheckContactCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateContactCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _contactService.GenerateContactCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}

