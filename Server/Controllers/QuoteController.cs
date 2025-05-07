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
    public class QuoteController
    (
    IPartnerService partnerService,
    IQuoteService quoteService,
    IEmployeeService employeeService) : ControllerBase
    {
        [HttpGet("quotes")]
        public async Task<IActionResult> GetAllQuotes([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.GetAllQuotesAsync(employee, partner, pageNumber, pageSize);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuoteById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.GetQuoteByIdAsync(id, employee, partner);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateQuote(CreateQuoteDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.CreateQuoteAsync(request, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateQuote(int id, UpdateQuoteDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.UpdateQuoteAsync(id, request, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }
        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteBulkQuotes(string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.DeleteBulkQuotesAsync(ids, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }

        [HttpPost("check-code")]
        public async Task<IActionResult> CheckQuoteCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await quoteService.CheckQuoteCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateOrderCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await quoteService.GenerateQuoteCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("{id:int}/orders")]
        public async Task<IActionResult> GetAllOrdersByQuote(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.GetAllOrdersByQuoteAsync(id, employee, partner);
            return Ok(result);
        }

        [HttpGet("{id:int}/activities")]
        public async Task<IActionResult> GetAllActivitiesByQuote(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.GetAllActivitiesByQuoteAsync(id, employee, partner);
            return Ok(result);
        }
        [HttpGet("{id:int}/activities-done")]
        public async Task<IActionResult> GetAllActivitiesDoneByQuote(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.GetAllActivitiesDoneByQuoteAsync(id, employee, partner);
            return Ok(result);
        }
        [HttpPut("{id:int}/activities/unassign")]
        public async Task<IActionResult> UnassignActivityFromId(int id, [FromBody] int activityId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            var result = await quoteService.UnassignActivityFromId(id, activityId, partner);
            return Ok(result);
        }
    }
}