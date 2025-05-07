using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Hubs;
using ServerLibrary.Services.Interfaces;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IHubContext<ChatHub> _hubContext;

        private readonly IPartnerService _partnerService;

        public ChatController(AppDbContext dbContext, IHubContext<ChatHub> hubContext, IPartnerService partnerService)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
            _partnerService = partnerService;
        }

        public class SendMessageRequest
        {
            public string UserId { get; set; }
            public string Message { get; set; }

            public string? TargetUserId { get; set; }
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var partner = await _partnerService.FindByClaim(identity);
                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Message))
                    return BadRequest("UserId và Message không được để trống!");

                // Gọi ChatHub để gửi tin nhắn
                await _hubContext.Clients.All.SendAsync("SendMessage", request.UserId, request.Message, request.TargetUserId);
                return Ok("Tin nhắn đã gửi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi SendMessage: {ex.Message}");
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }


        [HttpGet("connected")]
        public async Task<IActionResult> GetConnectedUsers()
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var partner = await _partnerService.FindByClaim(identity);
                await _hubContext.Clients.All.SendAsync("GetConnectedUsers", partner.Id.ToString());
                return Ok("Đã gửi yêu cầu lấy danh sách user!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi GetConnectedUsers: {ex.Message}");
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
        // [HttpPost("send")]
        // public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        // {
        //     try
        //     {
        //         // Kiểm tra user
        //         var user = await _dbContext.ApplicationUsers.FindAsync(int.Parse(request.UserId));
        //         if (user == null || !user.IsActive.GetValueOrDefault())
        //             return BadRequest("User không tồn tại hoặc không active!");

        //         // Lấy PartnerId
        //         var partnerId = await _dbContext.PartnerUsers
        //             .Where(pu => pu.UserId == int.Parse(request.UserId))
        //             .Select(pu => pu.PartnerId)
        //             .FirstOrDefaultAsync();

        //         if (partnerId == 0)
        //             return BadRequest("User không thuộc Partner nào!");

        //         // Lưu tin nhắn vào DB
        //         var chatMessage = new ChatMessage
        //         {
        //             UserId = int.Parse(request.UserId),
        //             PartnerId = partnerId,
        //             Message = request.Message,
        //             Timestamp = DateTime.UtcNow
        //         };
        //         _dbContext.ChatMessages.Add(chatMessage);
        //         await _dbContext.SaveChangesAsync();

        //         // Gửi tin nhắn qua SignalR
        //         var formattedMessage = $"{user.FullName}: {request.Message}";
        //         await _hubContext.Clients.Group($"Partner_{partnerId}")
        //             .SendAsync("ReceiveMessage", request.UserId, formattedMessage);

        //         return Ok("Tin nhắn đã gửi!");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Lỗi SendMessage: {ex.Message}");
        //         return StatusCode(500, $"Lỗi: {ex.Message}");
        //     }
        // }

        // [HttpGet("history")]
        // public async Task<IActionResult> GetChatHistory([FromQuery] int partnerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        // {
        //     try
        //     {
        //         // Kiểm tra PartnerId
        //         var partnerExists = await _dbContext.Partners.AnyAsync(p => p.Id == partnerId);
        //         if (!partnerExists)
        //             return BadRequest("Partner không tồn tại!");

        //         // Lấy lịch sử tin nhắn
        //         var messages = await _dbContext.ChatMessages
        //             .Where(m => m.PartnerId == partnerId)
        //             .Join(
        //                 _dbContext.ApplicationUsers,
        //                 msg => msg.UserId,
        //                 user => user.Id,
        //                 (msg, user) => new
        //                 {
        //                     msg.Id,
        //                     msg.UserId,
        //                     UserName = user.FullName,
        //                     msg.Message,
        //                     msg.Timestamp
        //                 })
        //             .OrderByDescending(m => m.Timestamp)
        //             .Skip((page - 1) * pageSize)
        //             .Take(pageSize)
        //             .ToListAsync();

        //         return Ok(messages);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Lỗi GetChatHistory: {ex.Message}");
        //         return StatusCode(500, $"Lỗi: {ex.Message}");
        //     }
        // }
    }

}