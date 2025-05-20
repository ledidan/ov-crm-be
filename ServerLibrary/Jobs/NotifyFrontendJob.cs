


using Microsoft.AspNetCore.SignalR;
using Quartz;
using ServerLibrary.Hubs;

namespace ServerLibrary.Jobs {
    public class NotifyFrontendJob : IJob
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotifyFrontendJob(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        string message = $"🕐 Ping từ server lúc: {DateTime.Now:HH:mm:ss}";
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
    }
}
}