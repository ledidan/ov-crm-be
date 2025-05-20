using Data.DTOs;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.MiddleWare;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;

namespace Server.Controllers
{
    [RequireValidLicense]
    [ApiController]
    [Route("api/[controller]")]
    public class RolePermissionController : ControllerBase
    {
        private readonly IRolePermissionService _service;
        private readonly IPartnerService _partnerService;

        private readonly IEmployeeService _employeeService;

        public RolePermissionController(IRolePermissionService service, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _service = service;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] CreateCRMRoleDto dto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Role name is required.");

            var response = await _service.CreateRoleAsync(dto, partner.Id);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateCRMRoleDTO updateCRMRoleDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var response = await _service.UpdateRoleAsync(updateCRMRoleDTO, partner.Id);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var response = await _service.DeleteRoleAsync(id, partner.Id);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id:int}/assign-permissions")]
        public async Task<IActionResult> AssignPermissions(int id, [FromBody] List<int> permissionIds)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var response = await _service.AssignPermissionsToRoleAsync(id, permissionIds, partner.Id);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPermissions(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var permissions = await _service.GetPermissionsForRoleAsync(id, partner.Id);
            if (permissions == null)
                return NotFound("No permissions found for the role.");
            return Ok(permissions);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetPermissionsForUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var permissions = await _service.GetRolesForEmployeeAsync(employee.Id, partner.Id);
            if (permissions == null)
                return NotFound("No permissions found for user's role.");
            return Ok(permissions);
        }


        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var roles = await _service.GetAllRolesAsync(partner);
            return Ok(roles);
        }

        [HttpGet("crm-permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var roles = await _service.GetAllPermissionsAsync();
            return Ok(roles);
        }
        [HttpPut("{id:int}/assign-users")]
        public async Task<IActionResult> AssignRoleForEmployeesAsync(int id, [FromBody] List<int> employeeIds)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var response = await _service.AssignRoleForEmployeesAsync(employeeIds, id, partner.Id);
            if(!response.Flag) return BadRequest(response);
            return Ok(response);
        }
    }
}