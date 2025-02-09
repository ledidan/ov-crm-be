using Data.DTOs.Order;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Orders>> GetAllAsync(Employee employee, Partner partner);

        Task<Orders?> GetOrderDetailAsync(string id, int employeeId);

        Task<GeneralResponse?> CreateOrderAsync(Orders orders, List<OrderDetails> orderDetails);

        Task<GeneralResponse?> UpdateOrderAsync(string orderId, OrderDTO orders, List<OrderDetailDTO> orderDetailsDTO);

        Task<GeneralResponse?> RemoveOrderAsync(string orderId, int employeeId);
    }
}
