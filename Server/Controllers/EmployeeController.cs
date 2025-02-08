using Data.DTOs;
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

        [HttpPost("create-employee")]
        [Authorize]
        public async Task<IActionResult> CreateEmployee(CreateEmployee employeeDTO)
        {
            if (employeeDTO == null) return BadRequest("Failed to create employee");
            var partner = await _partnerService.FindById(employeeDTO.PartnerId);
            if (partner == null)
            {
                return BadRequest("Partner not found");
            }
            var result = await _employeeService.CreateAsync(employeeDTO);
            return Ok(result);
        }

        [HttpGet("get-employees/{partnerId:int}")]
        [Authorize(Roles = "Admin, SysAdmin")]
        public async Task<IActionResult> GetAllEmployeeAsync([FromRoute] int partnerId)
        {
            var employees = await _employeeService.GetAllAsync(partnerId);
            if (employees == null || !employees.Any())
            {
                return NotFound("No employees found for the specified PartnerId.");
            }
            var employeeDTO = employees.Select(e => e.ToAllEmployeeDTO()).ToList();
            return Ok(employeeDTO);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Admin,SysAdmin")]
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
    }

}