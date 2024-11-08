using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<GeneralResponse> CreateAsync(CreateCustomer customer);
        Task<GeneralResponse> UpdateAsync(Customer customer);
        Task<List<Customer>> GetAllAsync(Partner partner);
    }
}
