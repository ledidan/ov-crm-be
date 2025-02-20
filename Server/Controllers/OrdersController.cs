
using Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Implementations;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _ordersService;

        private readonly IEmployeeService _employeeService;

        private readonly IPartnerService _partnerService;

        public OrdersController(IOrderService ordersService, IPartnerService partnerService, IEmployeeService employeeService)
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
        [HttpGet("get-all")]
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
        public async Task<IActionResult> UpdateOrderAsync(int id,
        [FromBody] UpdateOrderDTO request
         )
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (id == null)
            {
                return BadRequest("Order ID cannot be null or empty.");
            }
            var result = await _ordersService.UpdateOrderAsync(id, request.Order, employee, partner);

            if (result.Flag == true)
            {
                return Ok(result);
            }
            return NotFound($"Order with ID {id} not found.");
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderDTO request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (request.Order == null)
                return BadRequest(new { message = "Invalid request data" });

            if (employee == null || partner == null)
                return Unauthorized(new { message = "Unauthorized access" });

            var result = await _ordersService.UpdateFieldIdAsync(id, request.Order, employee, partner);

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
        [HttpPut("bulk-update")]
        public async Task<IActionResult> BulkUpdateOrders([FromBody] BulkOrderUpdateRequest request)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (request.OrderIds == null || !request.OrderIds.Any())
            {
                return BadRequest("No order IDs provided.");
            }

            var response = await _ordersService.BulkUpdateOrdersAsync(
                request.OrderIds,
                request.ContactId,
                request.CustomerId,
                employee,
                partner
            );

            if (response == null || !response.Flag)
            {
                return BadRequest(response?.Message ?? "Lỗi cập nhật đơn hàng.");
            }

            return Ok(response);
        }
    }
}
