


using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly AppDbContext _context;
        private readonly IEmployeeService _employeeService;

        private readonly ILogger<RolePermissionService> _logger;
        private readonly string[] _protectedRoles = { "Admin", "Employee", "Shipper" };

        public RolePermissionService(AppDbContext context, IEmployeeService employeeService, ILogger<RolePermissionService> logger)
        {
            _context = context;
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task<CRMRole> GetRoleAsync(int roleId, int? partnerId)
        {
            return await _context.CRMRoles
            .Where(r => r.Id == roleId && (partnerId == null || r.PartnerId == partnerId))
            .FirstOrDefaultAsync();
        }

        public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            var exists = await _context.CRMRolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (exists) return false;

            _context.CRMRolePermissions.Add(new CRMRolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var entity = await _context.CRMRolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (entity == null) return false;

            _context.CRMRolePermissions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        #region Create Role

        public async Task<GeneralResponse> CreateRoleAsync(CreateCRMRoleDto createRoleDto, int partnerId)
        {
            if (string.IsNullOrEmpty(createRoleDto.Name))
                return new GeneralResponse(false, "Role name cannot be empty.");

            // Check if role already exists
            var existingRole = await _context.CRMRoles
                .FirstOrDefaultAsync(r => r.Name == createRoleDto.Name && r.PartnerId == partnerId);

            if (existingRole != null)
                return new GeneralResponse(false, "Role already exists.");

            List<CRMRolePermission> sourcePermissions = new();
            if (createRoleDto.SourceRoleId.HasValue)
            {
                var sourceRole = await _context.CRMRoles
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == createRoleDto.SourceRoleId.Value && r.PartnerId == partnerId);

                if (sourceRole == null)
                    return new GeneralResponse(false, "Source role not found or does not belong to the specified partner.");

                sourcePermissions = sourceRole.RolePermissions.ToList();

            }
            var role = new CRMRole
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description,
                PartnerId = partnerId,
                RolePermissions = sourcePermissions.Any()
                    ? sourcePermissions.Select(rp => new CRMRolePermission
                    {
                        PermissionId = rp.PermissionId,
                    }).ToList()
                    : new List<CRMRolePermission>()
            };
            await _context.InsertIntoDb(role);
            var message = createRoleDto.SourceRoleId.HasValue
                        ? $"Role '{role.Name}' created successfully with permissions copied from source role."
                        : $"Role '{role.Name}' created successfully.";
            return new GeneralResponse(true, message);
        }

        #endregion

        #region Update Role

        public async Task<GeneralResponse> UpdateRoleAsync(UpdateCRMRoleDTO updateRoleDto, int partnerId)
        {
            if (string.IsNullOrEmpty(updateRoleDto.Name))
                return new GeneralResponse(false, "Role name cannot be empty.");

            var role = await _context
            .CRMRoles
            .Where(r => r.PartnerId == partnerId && r.Id == updateRoleDto.Id)
            .FirstOrDefaultAsync();

            if (role == null)
                return new GeneralResponse(false, "Role not found.");

            role.Name = updateRoleDto.Name;
            role.Description = updateRoleDto.Description;
            await _context.UpdateDb(role);

            return new GeneralResponse(true, $"Role '{updateRoleDto.Name}' updated successfully.");
        }

        #endregion

        #region Delete Role

        public async Task<GeneralResponse> DeleteRoleAsync(int roleId, int partnerId)
        {
            var role = await _context
            .CRMRoles
            .Where(r => r.PartnerId == partnerId && r.Id == roleId)
            .FirstOrDefaultAsync();

            if (role == null)
                return new GeneralResponse(false, "Role not found.");

            var roleHasUsers = await _context.Employees.AnyAsync(e => e.CRMRoleId == roleId);
            if (roleHasUsers)
                return new GeneralResponse(false, "Role is assigned to users, cannot be deleted.");
            if (_protectedRoles.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
                return new GeneralResponse(false, $"Role '{role.Name}' cannot be deleted.");

            _context.CRMRolePermissions.RemoveRange(role.RolePermissions);
            _context.CRMRoles.Remove(role);
            await _context.SaveChangesAsync();

            return new GeneralResponse(true, $"Role '{role.Name}' deleted successfully.");
        }

        #endregion

        #region Manage Role Permissions

        public async Task<GeneralResponse> AssignPermissionsToRoleAsync(int roleId, List<int> permissionIds, int partnerId)
        {
            if (permissionIds == null || !permissionIds.Any())
            {
                return new GeneralResponse(false, "No permissions provided.");
            }
            permissionIds = permissionIds.Distinct().ToList();
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var role = await _context.CRMRoles
                        .Where(r => r.Id == roleId && r.PartnerId == partnerId)
                        .FirstOrDefaultAsync();

                    if (role == null)
                    {
                        return new GeneralResponse(false, "Role not found.");
                    }

                    var permissions = await _context.CRMPermissions
                        .Where(p => permissionIds.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToListAsync();

                    if (permissions.Count != permissionIds.Count)
                    {
                        return new GeneralResponse(false, "Some permissions are invalid.");
                    }

                    var existingPermissionIds = await _context.CRMRolePermissions
                        .Where(rp => rp.RoleId == roleId)
                        .Select(rp => rp.PermissionId)
                        .ToListAsync();

                    var permissionsToRemove = permissionIds.Intersect(existingPermissionIds).ToList();
                    var permissionsToAdd = permissionIds.Except(existingPermissionIds).ToList();

                    if (permissionsToRemove.Any())
                    {
                        var rolePermissionsToRemove = await _context.CRMRolePermissions
                            .Where(rp => rp.RoleId == roleId && permissionsToRemove.Contains(rp.PermissionId))
                            .ToListAsync();
                        _context.CRMRolePermissions.RemoveRange(rolePermissionsToRemove);
                    }

                    if (permissionsToAdd.Any())
                    {
                        var newRolePermissions = permissionsToAdd.Select(id => new CRMRolePermission
                        {
                            RoleId = roleId,
                            PermissionId = id
                        }).ToList();
                        await _context.CRMRolePermissions.AddRangeAsync(newRolePermissions);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new GeneralResponse(true, $"Toggled {permissionsToRemove.Count} removed and {permissionsToAdd.Count} added permissions for role successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(false, $"Failed to toggle permissions: {ex.Message}");
                }
            });
        }

        #endregion

        #region Get Role Permissions

        private async Task<bool> CheckPermissionRoleExisted(int roleId)
        {
            return await _context.CRMRoles.AnyAsync(r => r.Id == roleId);
        }
        public async Task<List<RolePermissionsResponse>> GetPermissionsForRoleAsync(int roleId, int? partnerId)
        {
            _logger?.LogInformation("Fetching permissions for role ID {RoleId} with partner ID {PartnerId}", roleId, partnerId);

            // Check role existence
            var isRoleExisted = await CheckPermissionRoleExisted(roleId);
            if (!isRoleExisted)
            {
                _logger?.LogWarning("Role ID {RoleId} not found.", roleId);
                throw new Exception("Role not found");
            }

            // Get role
            var role = await GetRoleAsync(roleId, partnerId);
            if (role == null)
            {
                _logger?.LogWarning("Role ID {RoleId} not found for partner ID {PartnerId}.", roleId, partnerId);
                throw new Exception("Role not found or does not belong to the specified partner");
            }

            // Query role permissions
            var query = _context.CRMRolePermissions
                .Where(rp => rp.RoleId == roleId && rp.Role != null && rp.Permission != null)
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .AsQueryable();

            if (partnerId.HasValue)
            {
                query = query.Where(rp => rp.Role.PartnerId == partnerId.Value);
            }

            var rolePermissions = await query.ToListAsync();
            _logger?.LogInformation("Found {Count} role permissions for role ID {RoleId}.", rolePermissions.Count, roleId);

            // Map permissions
            var permissions = rolePermissions
                .Select(rp => new CRMPermission
                {
                    Id = rp.Permission.Id,
                    Action = rp.Permission.Action,
                    Resource = rp.Permission.Resource
                })
                .DistinctBy(p => p.Id)
                .ToList();

            // Build response
            var result = new RolePermissionsResponse
            {
                Role = new CRMRoleDTO
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description ?? ""
                },
                Permissions = permissions
            };

            _logger?.LogInformation("Successfully fetched permissions for role ID {RoleId}.", roleId);
            return new List<RolePermissionsResponse> { result };
        }

        #endregion

        #region Get All Permission

        public async Task<List<CRMPermissionsDTO>> GetAllPermissionsAsync()
        {
            var list = await _context.CRMPermissions.ToListAsync();

            var result = list.Select(p => new CRMPermissionsDTO
            {
                Id = p.Id,
                Action = p.Action,
                Resource = p.Resource
            }).ToList();
            return result;
        }

        public async Task<List<CRMRoleDTO>> GetAllRolesAsync(Partner partner)
        {
            if (partner == null || partner.Id == 0)
                return new List<CRMRoleDTO>();
            var list = await _context.CRMRoles
                .Where(r => r.PartnerId == partner.Id)
                .ToListAsync();
            var result = list.Select(r => new CRMRoleDTO
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description ?? ""
            }).ToList();
            return result;
        }

        public async Task<List<RolePermissionsResponse>> GetRolesForEmployeeAsync(int employeeId, int? partnerId)
        {
            // Check employee
            var employee = await _employeeService.FindByIdAsync(employeeId);
            if (employee == null)
            {
                throw new Exception("Employee not found");
            }
            var roleId = employee.CRMRoleId ?? 0;
            if (roleId == 0)
            {
                throw new Exception("Employee has no assigned CRM role");
            }
            _logger?.LogInformation("Fetching permissions for role ID {RoleId} with partner ID {PartnerId}", roleId, partnerId);

            // Check role existence
            var isRoleExisted = await CheckPermissionRoleExisted(roleId);
            if (!isRoleExisted)
            {
                _logger?.LogWarning("Role ID {RoleId} not found.", roleId);
                throw new Exception("Role not found");
            }

            // Get role
            var role = await GetRoleAsync(roleId, partnerId);
            if (role == null)
            {
                _logger?.LogWarning("Role ID {RoleId} not found for partner ID {PartnerId}.", roleId, partnerId);
                throw new Exception("Role not found or does not belong to the specified partner");
            }

            // Query role permissions
            var query = _context.CRMRolePermissions
                .Where(rp => rp.RoleId == roleId && rp.Role != null && rp.Permission != null)
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .AsQueryable();

            if (partnerId.HasValue)
            {
                query = query.Where(rp => rp.Role.PartnerId == partnerId.Value);
            }

            var rolePermissions = await query.ToListAsync();
            _logger?.LogInformation("Found {Count} role permissions for role ID {RoleId}.", rolePermissions.Count, roleId);

            // Map permissions
            var permissions = rolePermissions
                .Select(rp => new CRMPermission
                {
                    Id = rp.Permission.Id,
                    Action = rp.Permission.Action,
                    Resource = rp.Permission.Resource
                })
                .DistinctBy(p => p.Id)
                .ToList();

            // Build response
            var result = new RolePermissionsResponse
            {
                Role = new CRMRoleDTO
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description ?? ""
                },
                Permissions = permissions
            };

            _logger?.LogInformation("Successfully fetched permissions for role ID {RoleId}.", roleId);
            return new List<RolePermissionsResponse> { result };
        }

        public async Task<DataObjectResponse> AssignRoleForEmployeesAsync(List<int> employeeIds, int roleId, int partnerId)
        {
            if (employeeIds == null || !employeeIds.Any())
            {
                _logger.LogWarning("No employee IDs provided for role assignment.");
                return new DataObjectResponse(false, "Danh sách nhân viên rỗng.");
            }

            // Check role existence
            var isRoleExisted = await CheckPermissionRoleExisted(roleId);
            if (!isRoleExisted)
            {
                _logger.LogWarning("Role ID {RoleId} not found.", roleId);
                return new DataObjectResponse(false, "Không tìm thấy vai trò.");
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var failedEmployees = new List<int>();
                    foreach (var employeeId in employeeIds)
                    {
                        // Check employee
                        var employee = await _employeeService.FindByIdAsync(employeeId);
                        if (employee == null || employee.PartnerId != partnerId)
                        {
                            _logger.LogWarning("Employee ID {EmployeeId} not found or does not belong to partner ID {PartnerId}.", employeeId, partnerId);
                            failedEmployees.Add(employeeId);
                            continue;
                        }
                        employee.CRMRoleId = roleId;
                        await _context.UpdateDb(employee);
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    if (failedEmployees.Any())
                    {
                        var message = $"Phân quyền thành công, nhưng không thể gán cho nhân viên: {string.Join(", ", failedEmployees)}.";
                        _logger.LogWarning(message);
                        return new DataObjectResponse(true, message);
                    }

                    _logger.LogInformation("Assigned role ID {RoleId} to {Count} employees.", roleId, employeeIds.Count);
                    return new DataObjectResponse(true, "Phân quyền cho nhân viên thành công.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error assigning role ID {RoleId} to employees.", roleId);
                    return new DataObjectResponse(false, "Lỗi khi phân quyền cho nhân viên. Vui lòng thử lại sau.");
                }
            });
        }
        #endregion
    }

}