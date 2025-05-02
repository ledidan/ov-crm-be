using Data.DTOs;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class DashboardService : BaseService, IDashboardService
    {
        private readonly IOrderService _orderService;

        private readonly AppDbContext _context;

        public DashboardService(
            IOrderService orderService,
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor
        )
            : base(context, httpContextAccessor)
        {
            _orderService = orderService;
            _context = context;
        }

        public async Task<OrderStats> GetOrderStatsForAllOrders(Employee employee, Partner partner)
        {
            var allOrders = await _orderService.GetOrdersStatsCalculation(employee, partner);

            if (allOrders == null || !allOrders.Any())
            {
                return new OrderStats();
            }

            var stats = new OrderStats
            {
                OrderCount = allOrders.Count(), // Số lượng đơn hàng
                TotalOrderValue = allOrders.Sum(o => o.SaleOrderAmount), // Giá trị đơn hàng
                TotalCollectedAmount = allOrders
                    .Where(o => o.IsPaid == true)
                    .Sum(o => o.SaleOrderAmount), // Thực thu đơn hàng
                Debt = allOrders.Where(o => o.IsPaid == false).Sum(o => o.SaleOrderAmount), // Công phí thu đơn hàng (công nợ)
                PurchasedItems = allOrders.SelectMany(o => o.OrderDetails).ToList(), // Danh sách hàng hóa
            };

            return stats;
        }

        public Task<List<TopSellingProduct>> GetTopSellingProductsAsync(Employee employee, Partner partner, int topN = 5)
        {
            throw new NotImplementedException();
        }
    }
}
