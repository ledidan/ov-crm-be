using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ICustomerService
    {

        Task<DataObjectResponse?> GenerateCustomerCodeAsync(Partner partner);
        Task<DataObjectResponse?> CheckCustomerCodeAsync(string code, Employee employee, Partner partner);
        Task<DataStringResponse> CreateAsync(CreateCustomer customer, Employee employee, Partner partner);
        Task<GeneralResponse?> UpdateAsync(int id, CustomerDTO updateCustomer, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateCustomerDTO updateCustomer, Employee employee, Partner partner);
        Task<PagedResponse<List<CustomerDTO?>>> GetAllAsync(Employee employee, Partner partner, int pageNumber, int pageSize);
        Task<GeneralResponse?> DeleteBulkCustomers(string ids, Employee employee, Partner partner);
        Task<OptionalCustomerDTO?> GetCustomerByIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse> DeleteAsync(int customerId, Employee employee, Partner partner);

        // ** Bulk Add Customer Relationship
        Task<GeneralResponse?> BulkAddContactsIntoCustomer(List<int> contactIds, int customerId, Employee employee, Partner partner);

        Task<GeneralResponse?> RemoveContactFromCustomer(int id, int contactId, Partner partner);


        Task<GeneralResponse?> UnassignActivityFromCustomer(int id, int activityId, Partner partner);

        Task<GeneralResponse?> UnassignOrderFromCustomer(int id, int orderId, Partner partner);

        Task<GeneralResponse?> UnassignInvoiceFromCustomer(int id, int invoiceId, Partner partner);


        Task<GeneralResponse?> UnassignTicketFromCustomer(int id, int ticketId, Partner partner);

        Task<GeneralResponse?> UnassignQuoteFromCustomer(int id, int quoteId, Partner partner);

        Task<GeneralResponse?> UnassignCustomerCareTicketFromCustomer(int id, int customerCareTicketId, Partner partner);

        // ** Import Data CSV

        Task<ImportResultDto<CustomerDTO>> ImportCustomerDataAsync(List<CustomerDTO> data, Employee employee, Partner partner);
        
        //  ** Bulk Get Customer Relationship
            
        Task<List<ContactDTO>> GetAllContactsByIdAsync(int id, Partner partner);

        Task<PagedResponse<List<ContactDTO>>> GetAllContactAvailableByCustomer(int id, Partner partner, int pageNumber, int pageSize);
        Task<PagedResponse<List<ActivityDTO>>> GetAllActivitiesByIdAsync(int id, Partner partner, int pageNumber, int pageSize);

        Task<PagedResponse<List<OptionalOrderDTO>>> GetAllOrdersByIdAsync(int id, Partner partner, int pageNumber, int pageSize);

        Task<PagedResponse<List<InvoiceDTO>>> GetAllInvoicesByIdAsync(int id, Partner partner, int pageNumber, int pageSize);

        Task<PagedResponse<List<QuoteDTO>>> GetAllQuotesByIdAsync(int id, Partner partner, int pageNumber, int pageSize);
        Task<PagedResponse<List<SupportTicketDTO>>> GetAllTicketsByIdAsync(int id, Partner partner, int pageNumber, int pageSize);
        Task<PagedResponse<List<CustomerCareTicketDTO>>> GetAllCustomerCaresByIdAsync(int id, Partner partner, int pageNumber, int pageSize);
    }
}
