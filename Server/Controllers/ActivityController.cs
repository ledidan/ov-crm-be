using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
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


        [HttpGet("activities")]

        public async Task<IActionResult> GetAllActivities()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if (partner == null)
                return BadRequest("Model is empty");
            try
            {
                var result = await _activityService.GetAllActivityAsync(partner);
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

        [HttpPost("appointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDTO request)
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

            var response = await _activityService.CreateAppointmentAsync(request.Activity, request.Appointment, partner);
            if (response != null)
            {
                return Ok(response);
            }
            return BadRequest("Tạo lịch hẹn không thành công");
        }
        [HttpPost("call")]
        public async Task<IActionResult> CreateCall([FromBody] CreateCallRequestDTO request)
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

            var response = await _activityService.CreateCallAsync(request.Activity, request.Call, partner);

            if (response.Flag)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("mission")]
        public async Task<IActionResult> CreateMission([FromBody] CreateMissionRequestDTO request)
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

            var response = await _activityService.CreateMissionAsync(request.Activity, request.Mission, partner);
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
            // var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
            {
                return NotFound("Không tìm thấy tổ chức");
            }


            // if (employee == null)
            // {
            //     return NotFound("Employee not found");
            // }

            var appointment = await _activityService.GetByIdAsync(id, partner);
            if (appointment == null)
            {
                return NotFound();
            }
            return Ok(appointment);
        }
        [HttpPut("appointment")]
        public async Task<IActionResult> UpdateAppointment([FromBody] UpdateRequestAppointmentDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            // var employee = await _employeeService.FindByClaim(identity);
            if (partner == null)
                return NotFound("Không tìm thấy tổ chức");
            // if (employee == null)
            //     return NotFound("Không tìm thấy nhân viên");

            var result = await _activityService.
            UpdateAppointmentByIdAsync(request.Activity.Id, request.Activity, request.Appointment, partner);
            if (result == null || !result.Flag)
                return BadRequest(result?.Message ?? "Cập nhật lịch hẹn thất bại");

            return Ok(result);
        }

        [HttpPut("call")]
        public async Task<IActionResult> UpdateCall([FromBody] UpdateRequestCallDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức");
            if (employee == null)
                return NotFound("Không tìm thấy nhân viên");

            var result = await _activityService.UpdateCallByIdAsync(request.Activity.Id, request.Activity, request.Call, partner);
            if (result == null || !result.Flag)
                return BadRequest(result?.Message ?? "Cập nhật cuộc gọi thất bại");

            return Ok(result);
        }

        [HttpPut("mission")]
        public async Task<IActionResult> UpdateMission([FromBody] UpdateRequestMissionDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức");
            if (employee == null)
                return NotFound("Không tìm thấy nhân viên");

            var result = await _activityService
            .UpdateMissionByIdAsync(request.Activity.Id,
            request.Activity, request.Mission, partner);

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

        [HttpDelete("{id:int}/delete")]
        public async Task<IActionResult> DeleteActivityById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var response = await _activityService.DeleteIdAsync(id, employee, partner);
            return Ok(response);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateActivityById(int id, [FromBody] UpdateActivityDTO updateActivityDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var response = await _activityService.UpdateActivityIdAsync(id, updateActivityDTO, partner);
            return Ok(response);
        }
    }
}
