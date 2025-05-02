using Data.DTOs;
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
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IPartnerService _partnerService;

        private readonly IEmployeeService _employeeService;
        public CustomerController(ICustomerService customerService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _customerService = customerService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCustomerAsync(CreateCustomer customer)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            // Call the service to create the contact
            var result = await _customerService.CreateAsync(customer, employee, partner);
            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetAllAsync([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
                return BadRequest("Model is empty");
            try
            {
                var result = await _customerService.GetAllAsync(employee, partner, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Please check employeeId and partnerId correctly.Try again later." });
            }
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerIdAsync([FromRoute] int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var customer = await _customerService.GetCustomerByIdAsync(id, employee, partner);
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found for employeeId {employee.Id} and partnerId {partner.Id}.");
            }
            return Ok(customer);
        }
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateCustomerAsync([FromRoute] int id,
   [FromBody] CustomerDTO updateCustomer)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (id == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            var customer = await _customerService.UpdateAsync(id, updateCustomer, employee, partner);

            if (!customer.Flag)
            {
                return BadRequest(customer.Message);
            }
            return Ok(customer);

        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDTO updateCustomer)
        {
            if (updateCustomer == null)
                return BadRequest(new { message = "Invalid request data" });

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (employee == null || partner == null)
                return Unauthorized(new { message = "Unauthorized access" });

            var result = await _customerService.UpdateFieldIdAsync(id, updateCustomer, employee, partner);

            if (result == null || !result.Flag)
                return NotFound(new { message = result?.Message ?? "Customer not found" });

            return Ok(new { Flag = result.Flag, Message = result.Message });
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCustomerAsync
        ([FromRoute] int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _customerService.DeleteAsync(id, employee, partner);
            return Ok(result);
        }

        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteMultipleCustomers([FromQuery] string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var response = await _customerService.DeleteBulkCustomers(ids, employee, partner);

            if (!response.Flag)
            {
                return response.Message.Contains("not found") ? NotFound(response) :
                       response.Message.Contains("authorized") ? Unauthorized(response) :
                       BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("{id:int}/contacts")]
        public async Task<IActionResult> BulkAddContactsIntoCustomer([FromRoute] int id, [FromBody] List<int> contactIds)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null || contactIds == null || !contactIds.Any())
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.BulkAddContactsIntoCustomer(contactIds, id, employee, partner);
            if (response == null || !response.Flag)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id:int}/contacts-linked")]
        public async Task<IActionResult> GetAllContactsByIdAsync(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("ID khách hàng không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllContactsByIdAsync(id, partner);
            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id:int}/contacts-available")]
        public async Task<IActionResult> GetAllContactsAvailableByIdAsync(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            var partner = await _partnerService.FindByClaim(identity);

            if (id == null)
                return BadRequest("ID khách hàng không được để trống!");

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllContactAvailableByCustomer(id, partner, pageNumber, pageSize);
            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id:int}/activities")]

        public async Task<IActionResult> GetAllActivitiesByIdAsync(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllActivitiesByIdAsync(id, partner, pageNumber, pageSize);

            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpGet("{id:int}/orders")]

        public async Task<IActionResult> GetAllOrdersByIdAsync(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllOrdersByIdAsync(id, partner, pageNumber, pageSize);

            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpGet("{id:int}/invoices")]

        public async Task<IActionResult> GetAllInvoicesByIdAsync(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllInvoicesByIdAsync(id, partner, pageNumber, pageSize);

            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }


        [HttpDelete("{id:int}/contact")]
        public async Task<IActionResult> RemoveContactFromCustomer(int id, [FromBody] int contactId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.RemoveContactFromCustomer(id, contactId, partner);

            if (response.Flag == false)
                return BadRequest(response.Message);

            return Ok(response);

        }
        [HttpPut("{id:int}/activity/unassign")]
        public async Task<IActionResult> UnassignActivityFromCustomer(int id, [FromBody] int activityId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var response = await _customerService.UnassignActivityFromCustomer(id, activityId, partner);
            if (response.Flag == false)
                return BadRequest(response.Message);

            return Ok(response);
        }
        [HttpPut("{id:int}/order/unassign")]
        public async Task<IActionResult> UnassignOrderFromCustomer(int id, [FromBody] int orderId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var response = await _customerService.UnassignOrderFromCustomer(id, orderId, partner);
            if (response.Flag == false)
                return BadRequest(response.Message);

            return Ok(response);
        }
        [HttpPut("{id:int}/invoice/unassign")]
        public async Task<IActionResult> UnassignInvoiceFromCustomer(int id, [FromBody] int invoiceId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var response = await _customerService.UnassignInvoiceFromCustomer(id, invoiceId, partner);
            if (response.Flag == false)
                return BadRequest(response.Message);

            return Ok(response);
        }


        [HttpPost("check-code")]
        public async Task<IActionResult> CheckCustomerCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _customerService.CheckCustomerCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateCustomerCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _customerService.GenerateCustomerCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }
        [HttpGet("{id:int}/quotes")]
        public async Task<IActionResult> GetAllQuotesByCustomer(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var response = await _customerService.GetAllQuotesByIdAsync(id, partner, pageNumber, pageSize);
            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id:int}/supportTickets")]
        public async Task<IActionResult> GetAllSupportTicketsByCustomer(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var response = await _customerService.GetAllTicketsByIdAsync(id, partner, pageNumber, pageSize);
            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id:int}/customerCareTickets")]
        public async Task<IActionResult> GetAllCustomerCareTicketsByCustomer(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var response = await _customerService.GetAllCustomerCaresByIdAsync(id, partner, pageNumber, pageSize);
            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id:int}/quote/unassign")]
        public async Task<IActionResult> UnassignQuoteFromCustomer(int id, [FromBody] int quoteId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var response = await _customerService.UnassignQuoteFromCustomer(id, quoteId, partner);
            if (response.Flag == false)
                return BadRequest(response.Message);

            return Ok(response);
        }

        [HttpPut("{id:int}/supportTicket/unassign")]
        public async Task<IActionResult> UnassignSupportTicketFromCustomer(int id, [FromBody] int ticketId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var response = await _customerService.UnassignTicketFromCustomer(id, ticketId, partner);
            if (response.Flag == false)
                return BadRequest(response.Message);

            return Ok(response);
        }

        [HttpPut("{id:int}/customerCareTicket/unassign")]
        public async Task<IActionResult> UnassignCustomerCareTicketFromCustomer(int id, [FromBody] int customerCareTicketId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var response = await _customerService.UnassignCustomerCareTicketFromCustomer(id, customerCareTicketId, partner);
            if (response.Flag == false)
                return BadRequest(response.Message);

            return Ok(response);
        }
    }
}