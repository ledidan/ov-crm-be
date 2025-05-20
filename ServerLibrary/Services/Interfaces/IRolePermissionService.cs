using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IRolePermissionService
    {
        Task<GeneralResponse> CreateRoleAsync(CreateCRMRoleDto createRoleDto, int partnerId);
        Task<GeneralResponse> UpdateRoleAsync(UpdateCRMRoleDTO updateRoleDto, int partnerId);
        Task<GeneralResponse> DeleteRoleAsync(int roleId, int partnerId);
        Task<GeneralResponse> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds, int partnerId);
        Task<List<RolePermissionsResponse>> GetPermissionsForRoleAsync(int roleId, int? partnerId);

        Task<List<RolePermissionsResponse>> GetRolesForEmployeeAsync(int employeeId, int? partnerId);

        Task<List<CRMRoleDTO>> GetAllRolesAsync(Partner partner);

        Task<List<CRMPermissionsDTO>> GetAllPermissionsAsync();


        Task<DataObjectResponse> AssignRoleForEmployeesAsync(List<int> employeeIds, int roleId, int partnerId);
    }
}