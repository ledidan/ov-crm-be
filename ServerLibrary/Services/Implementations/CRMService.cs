


using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class CRMService : ICRMService
    {
        private readonly AppDbContext _appDbContext;

        private readonly IMapper _mapper;
        private readonly IPartnerService _partnerService;
        private readonly IEmployeeService _employeeService;
        private readonly IUserService _userService;
        private readonly ILicenseCenterService _licenseCenterService;
        public CRMService(AppDbContext appDbContext,
        IPartnerService partnerService, IEmployeeService employeeService,
IUserService _userService,
        IMapper mapper, IHttpContextAccessor httpContextAccessor,
        ILicenseCenterService _licenseCenterService
        )
        {
            _appDbContext = appDbContext;
            _partnerService = partnerService;
            _employeeService = employeeService;
            _licenseCenterService = _licenseCenterService;
            _userService = _userService;
            _mapper = mapper;
        }

        private async Task<SystemRole?> CheckSystemRole(string role)
        {
            return await _appDbContext.SystemRoles.FirstOrDefaultAsync(r => r.Name!.ToLower().Equals(role.ToLower()));
        }



        public async Task<DataObjectResponse> FirstSetupCRMPartnerAsync(CreatePartner partner, int userId)
        {
            // Kiểm tra user đã có license active chưa
            var hasActiveLicense = await _licenseCenterService.IsLicenseActiveAsync(userId);
            if (hasActiveLicense == true)
            {
                return new DataObjectResponse(false, "User đã có license active.");
            }

            // Lấy thông tin user
            var applicationUser = await _userService.GetApplicationUserByIdAsync(userId);
            if (applicationUser == null)
            {
                return new DataObjectResponse(false, "Không tìm thấy user.");
            }

            // Lấy role hệ thống "Admin"
            var adminRole = await CheckSystemRole(Constants.Role.Admin);
            if (adminRole == null)
            {
                return new DataObjectResponse(false, "Không tìm thấy role hệ thống.");
            }

            // Tạo Partner
            var newPartner = await _partnerService.CreatePartnerAsync(partner);
            if (newPartner == null)
            { 
                return new DataObjectResponse(false, "Tạo partner thất bại.");
            }

            // Gán system role cho user
            await _appDbContext.InsertIntoDb(new UserRole
            {
                Role = adminRole,
                User = applicationUser
            });

            // Seed mặc định các role CRM và tạo employee admin
            var employee = await SeedDefaultEmployeeRolesAndAdminAsync(newPartner.Id, applicationUser);

            // Liên kết PartnerUser
            await _appDbContext.InsertIntoDb(new PartnerUser
            {
                User = applicationUser,
                Partner = newPartner,
                EmployeeId = employee.Id
            });

            return new DataObjectResponse(true, "Khởi tạo CRM partner thành công.", newPartner);
        }


        public async Task<EmployeeDTO> SeedDefaultEmployeeRolesAndAdminAsync(int partnerId, ApplicationUser adminUser)
        {
            var allPermissions = await _appDbContext.CRMPermissions.ToListAsync();

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

            _appDbContext.CRMRolePermissions.AddRange(rolePermissions);
            await _appDbContext.SaveChangesAsync();

            var employeeData = new CreateEmployee
            {
                // ** Manually Enter 
                FullName = adminUser.FullName,
                Email = adminUser.Email,
                // ** Auto-generated
                EmployeeCode = "NV0000001",
                CRMRoleId = adminRole.Id,
                PartnerId = partnerId
            };
            return await _employeeService.CreateEmployeeAdminAsync(employeeData);
        }


    }
}