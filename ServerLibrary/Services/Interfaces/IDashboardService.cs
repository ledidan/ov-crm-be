using Data.DTOs;
using Data.Entities;

namespace ServerLibrary.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<OrderStats> GetOrderStatsForAllOrders(Employee employee, Partner partner); // Tổng đơn hàng
        Task<List<TopSellingProduct>> GetTopSellingProductsAsync(
            Employee employee,
            Partner partner,
            int topN = 5
        );
    }
}
