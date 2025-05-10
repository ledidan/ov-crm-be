
using System.Net;
using Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;


namespace ServerLibrary.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentStrategy _vnpayStrategy;
        public PaymentController(
            [FromKeyedServices("vnpay")] IPaymentStrategy vnpayStrategy
        )
        {
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
                await _vnpayStrategy.HandleWebhook(null); 
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { RspCode = "97", Message = $"Invalid signature or error: {ex.Message}" });
            }
        }

    }
}