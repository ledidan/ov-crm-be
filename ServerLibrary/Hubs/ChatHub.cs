using Data.Entities;
using Microsoft.AspNetCore.SignalR;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace ServerLibrary.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _dbContext;
        private readonly IPartnerService _partnerService;
        private static readonly ConcurrentDictionary<string, (string UserId, string FullName)> _connections = new();
        public ChatHub(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendMessage(Partner partner, string userId, string message, string? targetUserId = null)
        {
            try
            {
                var user = await _dbContext.ApplicationUsers.FindAsync(int.Parse(userId));
                if (user == null || !user.IsActive.GetValueOrDefault())
                    throw new Exception("User không tồn tại hoặc không active!");
                var partnerDetail = await _partnerService.FindById(partner.Id);
                if (partnerDetail == null)
                    throw new Exception("User không thuộc Partner nào!");

                var formattedMessage = $"{user.FullName}: {message}";

                if (!string.IsNullOrEmpty(targetUserId))
                {
                    // Chat 1-1: Gửi đến targetUserId
                    var targetUser = await _dbContext.ApplicationUsers.FindAsync(int.Parse(targetUserId));
                    if (targetUser == null || !targetUser.IsActive.GetValueOrDefault())
                        throw new Exception("User đích không tồn tại hoặc không active!");

                    var targetConnectionIds = _connections
                        .Where(c => c.Value.UserId == targetUserId)
                        .Select(c => c.Key)
                        .ToList();

                    if (!targetConnectionIds.Any())
                        throw new Exception("User kia không online!");

                    await Clients.Clients(targetConnectionIds)
                        .SendAsync("ReceiveMessage", userId, formattedMessage);
                    // Gửi lại cho chính sender
                    await Clients.Caller.SendAsync("ReceiveMessage", userId, formattedMessage);
                }
                else
                {
                    // Chat nhóm: Gửi đến Partner_{partnerId}
                    await Clients.Group($"Partner_{partnerDetail.Id}")
                        .SendAsync("ReceiveMessage", userId, formattedMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi SendMessage: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", $"Lỗi gửi tin nhắn: {ex.Message}");
            }
        }

        public async Task JoinPartnerGroup(Partner partner)
        {
            try
            {
                // Lấy PartnerId của user
                var partnerId = await _partnerService.FindById(partner.Id);

                if (partnerId == null)
                    throw new Exception("User không thuộc Partner nào!");

                // Thêm user vào nhóm Partner
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Partner_{partnerId}");
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Bạn đã tham gia nhóm chat Partner_{partnerId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi JoinPartnerGroup: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", $"Lỗi tham gia nhóm: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;
            Console.WriteLine($"User connected: {userId}, ConnectionId: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.ConnectionId;
            Console.WriteLine($"User disconnected: {userId}, Error: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}