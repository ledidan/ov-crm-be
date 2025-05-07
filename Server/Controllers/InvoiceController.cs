using Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.MiddleWare;
using ServerLibrary.Services.Implementations;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Server.Controllers
{
    [RequireValidLicense]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        private readonly IEmployeeService _employeeService;

        private readonly IPartnerService _partnerService;

        public InvoiceController(IInvoiceService invoiceService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _invoiceService = invoiceService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetAllInvoiceAsync([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _invoiceService.GetAllInvoicesAsync(employee, partner, pageNumber, pageSize);

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateInvoiceAsync(CreateInvoiceDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _invoiceService.CreateInvoiceAsync(request.Invoice, employee, partner);

            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateInvoiceAsync(int id, UpdateInvoiceDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _invoiceService.UpdateInvoiceAsync(id, request.Invoice, employee, partner);

            if (result.Flag == true)
            {
                return Ok(result);
            }
            return StatusCode(500, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInvoiceDetailAsync(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Invoice ID cannot be null or empty.");
            }

            var invoice = await _invoiceService
            .GetInvoiceByIdAsync(id, employee, partner);
            return Ok(invoice);
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceDTO invoiceDTO)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (invoiceDTO == null)
                return BadRequest(new { message = "Dữ liệu không đúng định dạng" });

            if (employee == null || partner == null)
                return Unauthorized(new { message = "Bạn không có quyền truy cập vào hoá đơn này" });

            var result = await _invoiceService.UpdateFieldIdAsync(id, invoiceDTO, employee, partner);

            if (result == null || !result.Flag)
                return NotFound(new { message = result?.Message ?? "Không tìm thấy hoá đơn" });

            return Ok(new { Flag = result.Flag, Message = result.Message });
        }
        [HttpDelete("bulk-delete")]

        public async Task<IActionResult> DeleteBulkInvoicesAsync(string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (ids == null)
            {
                return BadRequest("Cannot found invoice ids");
            }

            var result = await _invoiceService.DeleteBulkInvoicesAsync(ids, employee, partner);

            if (result.Flag == true)
            {
                return Ok(result);
            }
            return BadRequest("Failed to remove invoice");
        }

        [HttpPut("bulk-update")]
        public async Task<IActionResult> BulkUpdateInvoices([FromBody] BulkInvoiceUpdateRequest request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (request.InvoiceIds == null || !request.InvoiceIds.Any())
            {
                return BadRequest("No invoice IDs provided.");
            }

            var response = await _invoiceService.BulkUpdateInvoicesAsync(
                request.InvoiceIds,
                request.ContactId,
                request.CustomerId,
                employee,
                partner
            );

            if (response == null || !response.Flag)
            {
                return BadRequest(response?.Message ?? "Lỗi cập nhật hoá đơn.");
            }

            return Ok(response);
        }

        [HttpGet("{id:int}/orders")]
        public async Task<IActionResult> GetOrdersInvoice(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            // var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("No invoice IDs provided.");
            }
            var orders = await _invoiceService.GetOrdersByInvoiceIdAsync(id, partner);
            return Ok(orders);
        }


        [HttpGet("{id:int}/activities")]
        public async Task<IActionResult> GetAllActivitiesByOrderIdAsync(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("No invoice IDs provided.");
            }
            var activities = await _invoiceService.GetAllActivitiesByIdAsync(id, employee, partner);
            return Ok(activities);
        }

        [HttpDelete("{id:int}/order")]
        public async Task<IActionResult> RemoveOrderFromInvoice(int id, [FromBody] int orderId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Không tìm thấy hoá đơn");
            }
            var result = await _invoiceService.RemoveOrderFromInvoiceAsync(id, orderId, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return BadRequest("Đã xảy ra lỗi trong khi xoá đơn hàng khỏi hoá đơn");
        }

        [HttpDelete("{id:int}/activity")]
        public async Task<IActionResult> RemoveActivityFromInvoice(int id, [FromBody] int activityId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Không tìm thấy hoá đơn");
            }
            var result = await _invoiceService.RemoveActivityFromInvoiceAsync(id, activityId, employee, partner);
            if (result.Flag == true)
            {
                return Ok(result);
            }
            return BadRequest("Đã xảy ra lỗi trong khi xoá hoạt động khỏi hoá đơn");
        }

        [HttpPost("check-code")]
        public async Task<IActionResult> CheckInvoiceCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _invoiceService.CheckInvoiceCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateInvoiceCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _invoiceService.GenerateInvoiceCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}