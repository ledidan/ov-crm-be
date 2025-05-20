


using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class CRMService : ICRMService
    {
        private readonly AppDbContext _appDbContext;

        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        private readonly IPartnerService _partnerService;

        private readonly IEmployeeService _employeeService;

        private readonly ILogger<CRMService> logger;

        private readonly IJobGroupService _jobGroupService;

        public CRMService(AppDbContext appDbContext,
IUserService userService,
IJobGroupService jobGroupService,
IPartnerService partnerService,
IEmployeeService employeeService,
ILogger<CRMService> _logger,
        IMapper mapper
        )
        {
            _appDbContext = appDbContext;
            _userService = userService;
            _partnerService = partnerService;
            _jobGroupService = jobGroupService;
            _employeeService = employeeService;
            logger = _logger;
            _mapper = mapper;
        }

        private async Task<SystemRole?> CheckSystemRole(string role)
        {
            return await _appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Name!.ToLower().Equals(role.ToLower()));
        }

        public async Task<DataObjectResponse> FirstSetupCRMPartnerAsync(int partnerId, int userId, int employeeId)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Check user
                    var applicationUser = await _userService.GetApplicationUserByIdAsync(userId);
                    if (applicationUser == null)
                    {
                        logger?.LogWarning("User ID {UserId} not found when initializing CRM.", userId);
                        return new DataObjectResponse(false, "Không tìm thấy user.");
                    }

                    // Check partner
                    var partner = await _partnerService.FindById(partnerId);
                    if (partner == null)
                    {
                        logger?.LogWarning("Partner ID {PartnerId} not found.", partnerId);
                        return new DataObjectResponse(false, "Không tìm thấy doanh nghiệp.");
                    }

                    if (partner.IsInitialized == true)
                    {
                        logger?.LogInformation("Partner {PartnerId} already initialized.", partnerId);
                        return new DataObjectResponse(false, "Doanh nghiệp đã được khởi tạo CRM.");
                    }

                    var result = await SeedDefaultRolesAndAdminAsync(partnerId, employeeId);
                    if (!result.Flag)
                    {
                        logger?.LogError("Failed to seed default roles: {Message}", result.Message);
                        await transaction.RollbackAsync();
                        return result;
                    }

                    // Create Default Job Structure
                    await _jobGroupService.CreateDefaultJobPosition(partner);
                    await _jobGroupService.CreateDefaultJobTitle(partner);

                    // Mark as initialized
                    partner.IsInitialized = true;
                    partner.InitializedAt = DateTime.UtcNow;
                    _appDbContext.Partners.Update(partner);

                    await _appDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    logger?.LogInformation("Partner {PartnerId} initialized successfully by user {UserId}", partnerId, userId);
                    return new DataObjectResponse(true, "Khởi tạo CRM partner thành công.", partnerId);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error initializing CRM for partner {PartnerId}", partnerId);
                    await transaction.RollbackAsync();
                    return new DataObjectResponse(false, $"Lỗi khi khởi tạo CRM: {ex.Message}");
                }
            });
        }

        public async Task<DataObjectResponse> SeedDefaultRolesAndAdminAsync(int partnerId, int employeeId)
        {
            try
            {
                var allPermissions = await _appDbContext.CRMPermissions.ToListAsync();
                var employee = await _employeeService.FindByIdAsync(employeeId);
                if (employee == null)
                {
                    return new DataObjectResponse(false, "Không tìm thấy nhân viên.");
                }
                var existingRoles = await _appDbContext.CRMRoles
                    .Where(r => r.PartnerId == partnerId)
                    .ToListAsync();

                if (existingRoles.Any())
                {
                    logger?.LogWarning("Default roles already exist for partner {PartnerId}", partnerId);
                    return new DataObjectResponse(true, "Vai trò mặc định đã tồn tại.");
                }
                var roles = new List<CRMRole>
        {
            new CRMRole { Name = "Admin", PartnerId = partnerId, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            new CRMRole { Name = "Employee", PartnerId = partnerId, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            new CRMRole { Name = "Shipper", PartnerId = partnerId, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow }
        };

                _appDbContext.CRMRoles.AddRange(roles);
                await _appDbContext.SaveChangesAsync();

                var adminRole = roles.First(r => r.Name == "Admin");

                var rolePermissions = allPermissions.Select(p => new CRMRolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = p.Id
                }).ToList();
                
                // ** Updated employee data;
                employee.CRMRoleId = adminRole.Id;
                employee.CRMRole = adminRole;

                _appDbContext.CRMRolePermissions.AddRange(rolePermissions);
                _appDbContext.Employees.Update(employee);

                logger.LogInformation("Updated CRM Role permision and updated Employee CRM Role ID");

                await _appDbContext.SaveChangesAsync();

                logger?.LogInformation("Default roles and permissions seeded for partner {PartnerId}", partnerId);

                return new DataObjectResponse(true, "Khởi tạo vai trò thành công.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding default roles for partner {PartnerId}", partnerId);
                return new DataObjectResponse(false, $"Lỗi khi khởi tạo vai trò: {ex.Message}");
            }
        }
    }
}