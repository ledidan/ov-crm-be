
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
        private readonly OrdersService _ordersService;

        private readonly IEmployeeService _employeeService;

        private readonly IPartnerService _partnerService;

        public OrdersController(OrdersService ordersService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _ordersService = ordersService;
            _partnerService = partnerService;
            _employeeService = employeeService;
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
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllOrders()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var result = await _ordersService.GetAllAsync(employee, partner);

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
