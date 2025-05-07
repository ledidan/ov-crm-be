using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeDTO> CreateEmployeeAdminAsync(CreateEmployee employee);
        Task<DataStringResponse> CreateEmployeeAsync(CreateEmployee employee, Partner partner);
        Task<Employee?> FindByIdAsync(int id);

        Task<bool> EmployeeExists(int id, int partnerId);

        Task<GeneralResponse> UpdateAsync(Employee employee);
        Task<PagedResponse<List<EmployeeDTO>>> GetAllAsync(Partner partner, int pageNumber, int pageSize);

        Task<Employee?> FindByClaim(ClaimsIdentity? claimsIdentity);

    }
}
