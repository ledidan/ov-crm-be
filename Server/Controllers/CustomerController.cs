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

        [HttpPost("create-customer")]
        public async Task<IActionResult> CreateCustomerAsync(CreateCustomer customer)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (customer == null)
                return BadRequest("Model is empty");

            // Call the service to create the contact
            var result = await _customerService.CreateAsync(customer, employee, partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("get-all")]
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
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
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

            if (customer == null)
            {
                return BadRequest(new { flag = false, message = "Invalid customer data provided." });
            }
            return Ok(new { flag = true, message = "Customer updated successfully.", data = customer });

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
    }
}