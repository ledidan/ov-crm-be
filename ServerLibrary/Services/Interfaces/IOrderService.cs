using Data.DTOs;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetAllOrdersAsync(Employee employee, Partner partner);

        Task<OrderDTO?> GetOrderByIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse?> CreateOrderAsync(OrderDTO orderDto, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, OrderDTO orders, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateOrderAsync(int id, OrderDTO orders, Employee employee, Partner partner);

        Task<GeneralResponse?> DeleteBulkOrdersAsync(string ids, Employee employee, Partner partner);

        Task<GeneralResponse?> BulkUpdateOrdersAsync(List<int> orderIds, int? ContactId, int? CustomerId,
         Employee employee, Partner partner);
        // Task<GeneralResponse?> RemoveOrderAsync(int orderId, int employeeId);
    }
}
