



using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly AppDbContext _appDbContext;

        public SupportTicketService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        
    }
}