using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<GeneralResponse> CreateAsync(CreateCustomer customer, Employee employee, Partner partner);
        Task<CustomerDTO?> UpdateAsync(int id, CustomerDTO updateCustomer, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, CustomerDTO updateCustomer, Employee employee, Partner partner);
        Task<List<Customer?>> GetAllAsync(Employee employee, Partner partner);
        Task<GeneralResponse?> DeleteBulkCustomers(string ids, Employee employee, Partner partner);
        Task<Customer?> GetCustomerByIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse> DeleteAsync(int customerId, Employee employee, Partner partner);

        
    }
}
