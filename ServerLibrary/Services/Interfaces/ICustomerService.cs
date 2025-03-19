using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<DataStringResponse> CreateAsync(CreateCustomer customer, Employee employee, Partner partner);
        Task<GeneralResponse?> UpdateAsync(int id, CustomerDTO updateCustomer, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, CustomerDTO updateCustomer, Employee employee, Partner partner);
        Task<List<Customer?>> GetAllAsync(Employee employee, Partner partner);
        Task<GeneralResponse?> DeleteBulkCustomers(string ids, Employee employee, Partner partner);
        Task<Customer?> GetCustomerByIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse> DeleteAsync(int customerId, Employee employee, Partner partner);


        // ** Bulk Add Customer Relationship
        Task<GeneralResponse?> BulkAddContactsIntoCustomer(List<int> contactIds, int customerId, Employee employee, Partner partner);

        Task<GeneralResponse?> RemoveContactFromCustomer(int id, int contactId, Partner partner);
        //  ** Bulk Get Customer Relationship

        Task<List<ContactDTO>> GetAllContactsByIdAsync(int id, Partner partner);

        Task<List<ContactDTO>> GetAllContactAvailableByCustomer(int id, Partner partner);
        Task<List<Activity>> GetAllActivitiesByIdAsync(int id, Partner partner);

        Task<List<OptionalOrderDTO>> GetAllOrdersByIdAsync(int id, Partner partner);

        Task<List<Invoice>> GetAllInvoicesByIdAsync(int id, Partner partner);


    }
}
