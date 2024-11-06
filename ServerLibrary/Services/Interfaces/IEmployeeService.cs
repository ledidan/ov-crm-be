using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<GeneralResponse> CreateAsync(CreateEmployee employee);
        Task<GeneralResponse> UpdateAsync(Employee employee);
        Task<List<Employee>> GetAllAsync(int partnerId);
    }
}
