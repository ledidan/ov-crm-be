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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OpportunityController
    (
    IPartnerService partnerService,
    IOpportunityService opportunityService,
    IEmployeeService employeeService) : ControllerBase
    {
        [HttpGet("opportunities")]
        public async Task<IActionResult> GetAllOpportunities([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await opportunityService.GetAllOpportunitiesAsync(employee, partner, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOpportunityById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await opportunityService.GetOpportunityByIdAsync(id, employee, partner);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateOpportunity(CreateOpportunityDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await opportunityService.CreateOpportunityAsync(request, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOpportunity(int id, UpdateOpportunityDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await opportunityService.UpdateOpportunityAsync(id, request, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }
        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteBulkOpportunities(string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await opportunityService.DeleteBulkOpportunitiesAsync(ids, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }

        [HttpPost("check-code")]
        public async Task<IActionResult> CheckOpportunityCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await opportunityService.CheckOpportunityCodeAsync(code, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }
        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateOpportunityCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            // var employee = await employeeService.FindByClaim(identity);
            var result = await opportunityService.GenerateOpportunityCodeAsync(partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }

        [HttpPost("{id:int}/contacts")]
        public async Task<IActionResult> BulkAddContactsIntoId([FromRoute] int id, [FromBody] List<int> contactIds)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null || contactIds == null || !contactIds.Any())
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await opportunityService.BulkAddContactsIntoId(contactIds, id, employee, partner);
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

            var partner = await partnerService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await opportunityService.GetAllContactsByIdAsync(id, partner);
            if (response == null)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id:int}/contact")]
        public async Task<IActionResult> RemoveContactFromOpportunity(int id, [FromBody] int contactId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await partnerService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await opportunityService.RemoveContactFromId(id, contactId, partner);

            if (response.Flag == false)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id:int}/order/unassign")]
        public async Task<IActionResult> UnassignOrdersFromId(int id, [FromBody] int orderId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await partnerService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await opportunityService.UnassignOrderFromId(id, orderId, partner);

            if (response.Flag == false)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id:int}/activity/unassign")]
        public async Task<IActionResult> UnassignActivityFromId(int id, [FromBody] int activityId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("ID Cơ hội không được để trống!");

            var partner = await partnerService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await opportunityService.UnassignActivityFromId(id, activityId, partner);

            if (response.Flag == false)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPut("{id:int}/quote/unassign")]
        public async Task<IActionResult> UnassignQuoteFromId(int id, [FromBody] int quoteId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null)
                return BadRequest("ID Cơ hội không được để trống!");

            var partner = await partnerService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await opportunityService.UnassignQuoteFromId(id, quoteId, partner);

            if (response.Flag == false)
                return BadRequest(response);

            return Ok(response);

        }
    }
}