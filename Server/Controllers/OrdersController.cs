using System.Security.Claims;
using System.Threading.Tasks;
using Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Implementations;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _ordersService;

        private readonly IEmployeeService _employeeService;

        private readonly IPartnerService _partnerService;

        public OrdersController(
            IOrderService ordersService,
            IPartnerService partnerService,
            IEmployeeService employeeService
        )
        {
            _ordersService = ordersService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }

        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (request == null || request.Order == null || request.Order.OrderDetails == null)
            {
                return BadRequest("Invalid request data.");
            }
            var response = await _ordersService.CreateOrderAsync(request.Order, employee, partner);
            if (response.Flag == true)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _ordersService.GetAllOrdersAsync(employee, partner);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderDetailAsync(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Order ID cannot be null or empty.");
            }

            var order = await _ordersService.GetOrderByIdAsync(id, employee, partner);
            return Ok(order);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrderAsync(
            int id,
            [FromBody] UpdateOrderRequestDTO request
        )
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Order ID cannot be null or empty.");
            }
            var result = await _ordersService.UpdateOrderAsync(
                id,
                request.Order,
                employee,
                partner
            );

            if (result.Flag == true)
            {
                return Ok(result);
            }
            return NotFound($"Order with ID {id} not found.");
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateFieldOrder(
            int id,
            [FromBody] UpdateOrderRequestDTO request
        )
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (request.Order == null)
                return BadRequest(new { message = "Invalid request data" });

            if (employee == null || partner == null)
                return Unauthorized(new { message = "Unauthorized access" });

            var result = await _ordersService.UpdateFieldIdAsync(
                id,
                request.Order,
                employee,
                partner
            );

            if (result == null || !result.Flag)
                return NotFound(new { message = result?.Message ?? "Order not found" });

            return Ok(new { Flag = result.Flag, Message = result.Message });
        }

        // [HttpDelete("{id:length(24)}")]
        // public async Task<IActionResult> DeleteOrderAsync(int id, int employeeId)
        // {
        //     if (id == null)
        //     {
        //         return BadRequest($"{id} not found");
        //     }

        //     var result = await _ordersService.RemoveOrderAsync(id, employeeId);

        //     if (result.Flag == true)
        //     {
        //         return Ok(result);
        //     }
        //     return BadRequest("Failed to remove order");
        // }

        [HttpDelete("bulk-delete")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteBulkOrdersAsync(string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (ids == null)
            {
                return BadRequest("Cannot found orders ids");
            }

            var result = await _ordersService.DeleteBulkOrdersAsync(ids, employee, partner);

            if (result.Flag == true)
            {
                return Ok(result);
            }
            return BadRequest("Failed to remove order");
        }

        [HttpPost("{id:int}/add-contacts")]
        public async Task<IActionResult> BulkAddContactsIntoOrder(
            [FromRoute] int id,
            [FromBody] List<int> ContactIds
        )
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (id == null || ContactIds == null || !ContactIds.Any())
                return BadRequest("Danh sách liên hệ không được để trống!");

            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null)
                return NotFound("Không tìm thấy tổ chức!");

            var response = await _ordersService.BulkAddContactsIntoOrder(
                ContactIds,
                id,
                employee,
                partner
            );
            if (response == null || !response.Flag)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("{id:int}/activities")]
        public async Task<IActionResult> GetAllActivitiesByOrder(int id)
        {
            if (id == null)
            {
                return BadRequest("Không tìm thấy đơn hàng");
            }
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null || employee == null)
                return BadRequest("ID nhân viên và ID tổ chức không đuọc bỏ trống.");
            var response = await _ordersService.GetAllActivitiesByOrderAsync(id, employee, partner);
            return Ok(response);
        }

        [HttpGet("{id:int}/contacts-linked")]
        public async Task<IActionResult> GetAllContactsLinked(int id)
        {
            if (id == null)
            {
                return BadRequest("Không tìm thấy đơn hàng");
            }
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null || employee == null)
                return BadRequest("ID nhân viên và ID tổ chức không đuọc bỏ trống.");
            var response = await _ordersService.GetAllContactsLinkedIdAsync(id, employee, partner);
            return Ok(response);
        }

        [HttpGet("{id:int}/contacts-available")]
        public async Task<IActionResult> GetAllContactsAvailable(int id)
        {
            if (id == null)
            {
                return BadRequest("Không tìm thấy đơn hàng");
            }
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null || employee == null)
                return BadRequest("ID nhân viên và ID tổ chức không đuọc bỏ trống.");
            var response = await _ordersService.GetAllContactsAvailableByIdAsync(
                id,
                employee,
                partner
            );
            return Ok(response);
        }

        [HttpDelete("{id:int}/invoice")]
        public async Task<IActionResult> RemoveInvoiceFromIdAsync(int id)
        {
            if (id == null)
            {
                return BadRequest("Không tìm thấy đơn hàng");
            }
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null || employee == null)
                return BadRequest("ID nhân viên và ID tổ chức không đuọc bỏ trống.");
            var response = await _ordersService.RemoveInvoiceFromIdAsync(id, employee, partner);
            return Ok(response);
        }

        [HttpGet("{id:int}/invoices")]
        public async Task<IActionResult> GetInvoiceFromIdAsync(int id)
        {
            if (id == null)
            {
                return BadRequest("Không tìm thấy đơn hàng");
            }

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("ID nhân viên và ID tổ chức không đuọc bỏ trống.");
            var response = await _ordersService.GetAllInvoicesAsync(id, employee, partner);
            return Ok(response);
        }

        [HttpPut("{id:int}/customer/unassign")]
        public async Task<IActionResult> UnassignCustomerFromOrder(
            int id,
            [FromBody] int customerId
        )
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Không tìm thấy đơn hàng");
            }
            var response = await _ordersService.UnassignCustomerFromOrder(
                id,
                customerId,
                employee,
                partner
            );
            if (response == null || !response.Flag)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("{id:int}/activity/unassign")]
        public async Task<IActionResult> UnassignActivityFromOrder(
            int id,
            [FromBody] int activityId
        )
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Không tìm thấy đơn hàng");
            }
            var response = await _ordersService.UnassignActivityFromOrder(id, activityId, partner);
            if (response == null || !response.Flag)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{id:int}/contact")]
        public async Task<IActionResult> RemoveContactFromOrder(int id, [FromBody] int contactId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Không tìm thấy đơn hàng");
            }
            var response = await _ordersService.RemoveContactFromOrder(
                id,
                contactId,
                employee,
                partner
            );
            if (response == null || !response.Flag)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{customerId}/order-stats")]
        public async Task<IActionResult> GetOrderStats(int customerId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var stats = await _ordersService.GetOrderStatsForCustomer(
                customerId,
                employee,
                partner
            );
            return Ok(stats);
        }
        [HttpPost("check-code")]
        public async Task<IActionResult> CheckOrderCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _ordersService.CheckOrderCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateOrderCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _ordersService.GenerateOrderCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}
