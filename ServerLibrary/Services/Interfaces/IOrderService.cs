using Data.DTOs;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IOrderService
    {

        Task<DataObjectResponse?> GenerateOrderCodeAsync(Partner partner);
        Task<DataObjectResponse?> CheckOrderCodeAsync(string code, Employee employee, Partner partner);

        Task<PagedResponse<List<OrderDTO>>> GetAllOrdersAsync(Employee employee, Partner partner, int pageNumber, int pageSize);
        Task<List<OrderDTO>> GetOrdersStatsCalculation(Employee employee, Partner partner);
        Task<OrderDTO?> GetOrderByIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse?> CreateOrderAsync(
            OrderDTO orderDto,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> UpdateFieldIdAsync(
            int id,
            UpdateOrderDTO orders,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> UpdateOrderAsync(
            int id,
            UpdateOrderDTO orders,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> DeleteBulkOrdersAsync(
            string ids,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> RemoveInvoiceFromIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse?> BulkAddContactsIntoOrder(
            List<int> ContactIds,
            int id,
            Employee employee,
            Partner Partner
        );

        Task<List<Activity?>> GetAllActivitiesByOrderAsync(
            int orderId,
            Employee employee,
            Partner partner
        );

        Task<PagedResponse<List<ContactDTO>>> GetAllContactsAvailableByIdAsync(
            int id,
            Employee employee,
            Partner partner,
            int pageNumber,
            int pageSize
        );

        Task<List<ContactDTO>> GetAllContactsLinkedIdAsync(
            int id,
            Employee employee,
            Partner partner
        );

        Task<OrderStats> GetOrderStatsForCustomer(
            int customerId,
            Employee employee,
            Partner partner
        );
        Task<List<InvoiceDTO>> GetAllInvoicesAsync(int id, Employee employee, Partner partner);

        //** Unassign Customer from Order
        Task<GeneralResponse?> UnassignCustomerFromOrder(
            int id,
            int customerId,
            Employee employee,
            Partner partner
        );

        //** Unassign Activity from Order
        Task<GeneralResponse?> UnassignActivityFromOrder(int id, int activityId, Partner partner);

        //** Remove Contact from Order
        Task<GeneralResponse?> RemoveContactFromOrder(
            int id,
            int contactId,
            Employee employee,
            Partner partner
        );
    }
}
