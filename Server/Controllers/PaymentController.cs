
using System.Net;
using System.Text.Json;
using Data.DTOs;
using Data.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerLibrary.MiddleWare;
using ServerLibrary.Patterns;

namespace ServerLibrary.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentStrategy _vnpayStrategy;
        public PaymentController(
            [FromKeyedServices("vnpay")] IPaymentStrategy vnpayStrategy
        ) {
            _vnpayStrategy = vnpayStrategy;
        }
        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] AppPaymentRequest request)
        {
  try
            {
                if (request == null || string.IsNullOrEmpty(request.PaymentMethod))
                {
                    return BadRequest("Yêu cầu thanh toán trống hoặc thiếu phương thức, check lại nha!");
                }

                IPaymentStrategy strategy = request.PaymentMethod.ToLower() switch
                {
                    "vnpay" => _vnpayStrategy,
                    _ => throw new Exception("Phương thức thanh toán lạ hoắc, chỉ hỗ trợ vnpay hoặc momo nha!")
                };

                var response = await strategy.CreatePayment(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo thanh toán: {ex.Message}");
                return StatusCode(500, $"{ex.Message}");
            }
        }


        [HttpGet("webhook/vnpay")]
        public async Task<IActionResult> HandleVnpayWebhook()
        {
            try
            {
                // Lấy query parameters từ webhook
                var queryParams = HttpContext.Request.Query
                    .ToDictionary(q => q.Key, q => q.Value.ToString());

                await _vnpayStrategy.HandleWebhook(queryParams);
                return Ok("Webhook xử lý ngon lành, cảm ơn VNPAY nha!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi webhook VNPAY: {ex.Message}");
                return StatusCode(500, $"{ex.Message}");
            }
        }
    }
}