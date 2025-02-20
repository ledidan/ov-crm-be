using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<GeneralResponse> CreateAsync(CreateEmployee employee);
        Task<Employee?> FindByIdAsync(int id);
        
        Task<bool> EmployeeExists(int id);
        Task<GeneralResponse> UpdateAsync(Employee employee);
        Task<List<Employee>> GetAllAsync(int partnerId);

        Task<Employee?> FindByClaim(ClaimsIdentity? claimsIdentity);
    }
}
