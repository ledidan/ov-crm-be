using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Mapper.EmployeeMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {

        private readonly IEmployeeService _employeeService;

        private readonly IPartnerService _partnerService;

        public EmployeeController(IEmployeeService employeeService, IPartnerService partnerService)
        {
            _employeeService = employeeService;
            _partnerService = partnerService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEmployee(CreateEmployee employee)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);

            if (employee == null) return BadRequest("Failed to create employee");
            if (partner == null)
            {
                return BadRequest("Không tìm thấy tổ chức");
            }
            var result = await _employeeService.CreateEmployeeAsync(employee, partner);
            if (!result.Flag)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPost("employeeAdmin")]
        public async Task<IActionResult> CreateEmployAdmin(CreateEmployee employee)
        {
            if (employee == null) return BadRequest("Request employee không hợp lệ");
            var partner = await _partnerService.FindById(employee.PartnerId);
            if (partner == null)
            {
                return BadRequest("Không tìm thấy tổ chức");
            }
            var result = await _employeeService.CreateEmployeeAdminAsync(employee);
            if (result == null)
            {
                return BadRequest("Tạo nhân viên cho Admin không thành công");
            }
            return Ok(result);
        }

        [HttpGet("employees")]
        [Authorize]
        public async Task<IActionResult> GetAllEmployeeAsync([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employees = await _employeeService.GetAllAsync(partner, pageNumber, pageSize);
            if (employees == null || !employees.Data.Any())
            {
                return NotFound("No employees found for the specified PartnerId.");
            }
            return Ok(employees);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetEmployeeById([FromRoute] int id)
        {
            var employee = await _employeeService.FindByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            try
            {
                return Ok(employee.ToEmployeeDTO());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while converting the employee to DTO: {ex.Message}");
            }
        }

        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateEmployeeCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _employeeService.GenerateEmployeeCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("check-code")]
        public async Task<IActionResult> CheckEmployeeCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _employeeService.CheckEmployeeCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }
    }

}