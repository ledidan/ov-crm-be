using Data.DTOs;
using Data.DTOs.Contact;
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

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateOrderDTO orders, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateOrderAsync(int id, UpdateOrderDTO orders, Employee employee, Partner partner);

        Task<GeneralResponse?> DeleteBulkOrdersAsync(string ids, Employee employee, Partner partner);


        Task<GeneralResponse> RemoveInvoiceFromIdAsync(int id, Employee employee, Partner partner);


        Task<GeneralResponse?> BulkAddContactsIntoOrder(List<int> ContactIds, int OrderId, Employee employee, Partner Partner);

        Task<List<Activity?>> GetAllActivitiesByOrderAsync(int orderId, Employee employee, Partner partner);

        Task<List<ContactDTO>> GetAllContactsAvailableByIdAsync(int id, Employee employee, Partner partner);

        Task<List<ContactDTO>> GetAllContactsLinkedIdAsync(int id, Employee employee, Partner partner);


        Task<List<InvoiceDTO>> GetAllInvoicesAsync(int id, Employee employee, Partner partner);
        // Task<GeneralResponse?> RemoveOrderAsync(int orderId, int employeeId);
    }
}
