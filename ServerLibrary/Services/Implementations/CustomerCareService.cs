


using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class CustomerCareService : ICustomerCareService
    {
        private readonly AppDbContext _appDbContext;

        public CustomerCareService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        
    }
}