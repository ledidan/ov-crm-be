using Data.DTOs.Order;
using Data.Enums;
using Data.MongoModels;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Implementations;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrdersService _ordersService;

        public OrdersController(OrdersService ordersService)
        {
            _ordersService = ordersService;
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderDTO request)
        {
            if (request == null || request.Order == null || request.OrderDetails == null)
            {
                return BadRequest("Invalid request data.");
            }
            var response = await _ordersService.CreateOrderAsync(request.Order, request.OrderDetails);
            if (response.Flag == true)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }
        [HttpGet("get-all-orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int employeeId, [FromQuery] int partnerId)
        {
            var result = await _ordersService.GetAllAsync(employeeId, partnerId);

            return Ok(result);
        }

        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> GetOrderDetailAsync(string id, int employeeId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Order ID cannot be null or empty.");
            }

            var order = await _ordersService.GetOrderDetailAsync(id, employeeId);
            return Ok(order);
        }
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> UpdateOrderAsync(string id,
        [FromBody] UpdateOrderDTO request
         )
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Order ID cannot be null or empty.");
            }
            var result = await _ordersService.UpdateOrderAsync(id, request.Order, request.OrderDetails);

            if (result.Flag == true)
            {
                return Ok(result);
            }
            return NotFound($"Order with ID {id} not found.");
        }


        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> DeleteOrderAsync(string id, int employeeId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest($"{id} not found");
            }

            var result = await _ordersService.RemoveOrderAsync(id, employeeId);

            if (result.Flag == true)
            {
                return Ok(result);
            }
            return BadRequest("Failed to remove order");
        }


    }
}
