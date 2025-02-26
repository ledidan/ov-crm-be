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
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;
        private readonly IPartnerService _partnerService;

        private readonly IEmployeeService _employeeService;
        public ActivityController(IActivityService activityService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _activityService = activityService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }


        [HttpGet("get-all")]

        public async Task<IActionResult> GetAllActivities()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
                return BadRequest("Model is empty");
            try
            {
                var result = await _activityService.GetAllActivityAsync(employee, partner);
                var resultDTO = result.ToList();
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

        [HttpPost("create-appointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO dto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (partner == null)
            {
                return NotFound("Partner not found");
            }

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            var response = await _activityService.CreateAppointmentAsync(dto, employee, partner);
            if (response.Flag)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }
        [HttpPost("create-call")]
        public async Task<IActionResult> CreateCall([FromBody] CreateCallDTO dto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (partner == null)
            {
                return NotFound("Partner not found");
            }

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            var response = await _activityService.CreateCallAsync(dto, employee, partner);
            if (response.Flag)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("create-mission")]
        public async Task<IActionResult> CreateMission([FromBody] CreateMissionDTO dto)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (partner == null)
            {
                return NotFound("Partner not found");
            }

            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            var response = await _activityService.CreateMissionAsync(dto, employee, partner);
            if (response.Flag)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
            {
                return NotFound("Partner not found");
            }


            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            var appointment = await _activityService.GetByIdAsync(id, employee, partner);
            if (appointment == null)
            {
                return NotFound();
            }
            return Ok(appointment);
        }
        [HttpPut("appointment/{id:int}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDTO updateAppointmentDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức");
            if (employee == null)
                return NotFound("Không tìm thấy nhân viên");

            var result = await _activityService.UpdateAppointmentByIdAsync(id, updateAppointmentDTO, employee, partner);
            if (result == null || !result.Flag)
                return BadRequest(result?.Message ?? "Cập nhật lịch hẹn thất bại");

            return Ok(result);
        }

        [HttpPut("call/{id:int}")]
        public async Task<IActionResult> UpdateCall(int id, [FromBody] UpdateCallDTO updateCallDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức");
            if (employee == null)
                return NotFound("Không tìm thấy nhân viên");

            var result = await _activityService.UpdateCallByIdAsync(id, updateCallDTO, employee, partner);
            if (result == null || !result.Flag)
                return BadRequest(result?.Message ?? "Cập nhật cuộc gọi thất bại");

            return Ok(result);
        }

        [HttpPut("mission/{id:int}")]
        public async Task<IActionResult> UpdateMission(int id, [FromBody] UpdateMissionDTO updateMissionDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức");
            if (employee == null)
                return NotFound("Không tìm thấy nhân viên");

            var result = await _activityService.UpdateMissionByIdAsync(id, updateMissionDTO, employee, partner);
            if (result == null || !result.Flag)
                return BadRequest(result?.Message ?? "Cập nhật nhiệm vụ thất bại");

            return Ok(result);
        }
         [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteBulkActivities([FromQuery] string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var response = await _activityService.DeleteBulkActivities(ids, employee, partner);
            return Ok(response);
        }
    }
}
