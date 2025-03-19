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
        public async Task<IActionResult> GetAllAsync()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
                return BadRequest("Model is empty");
            try
            {
                var result = await _customerService.GetAllAsync(employee, partner);
                var resultDTO = result.Select(x => x.ToCustomerDTO()).ToList();
                return Ok(resultDTO);
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
                return NotFound($"Contact with ID {id} not found for employeeId {employee.Id} and partnerId {partner.Id}.");
            }
            return Ok(customer.ToCustomerDTO());
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
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerDTO updateCustomer)
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
        public async Task<IActionResult> GetAllContactsAvailableByIdAsync(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            var partner = await _partnerService.FindByClaim(identity);

            if (id == null)
                return BadRequest("ID khách hàng không được để trống!");

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllContactAvailableByCustomer(id, partner);
            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id:int}/activities")]

        public async Task<IActionResult> GetAllActivitiesByIdAsync(int id)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllActivitiesByIdAsync(id, partner);

            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpGet("{id:int}/orders")]

        public async Task<IActionResult> GetAllOrdersByIdAsync(int id)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllOrdersByIdAsync(id, partner);

            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }
        [HttpGet("{id:int}/invoices")]

        public async Task<IActionResult> GetAllInvoicesByIdAsync(int id)
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _customerService.GetAllInvoicesByIdAsync(id, partner);

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
    }
}