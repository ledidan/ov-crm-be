using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<GeneralResponse> CreateAsync(CreateCustomer customer);
        Task<CustomerDTO?> UpdateAsync(int id, CustomerDTO updateCustomer);
        Task<List<Customer?>> GetAllAsync(Employee employee, Partner partner);
        Task<GeneralResponse?> DeleteBulkCustomers(string ids, int employeeId, int partnerId);
        Task<Customer?> GetCustomerByIdAsync(int id, int employeeId, int partnerId);

        Task<GeneralResponse> DeleteAsync(int customerId, int employeeId, int partnerId);
    }
}
