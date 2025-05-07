
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Data.Applications;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;

namespace ServerLibrary.Services.Implementations
{

    public class LicenseCenterService : ILicenseCenterService
    {

        private readonly AppDbContext _context;

        public LicenseCenterService(AppDbContext context)
        {

            _context = context;
        }

        // Application methods
        public async Task<List<Application>> GetApplicationsAsync() =>
            await _context.Applications.ToListAsync();

        public async Task<DataObjectResponse> CreateApplicationAsync(CreateApplicationDTO app)
        {
            var application = new Application
            {
                Name = app.Name,
                Description = app.Description
            };
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();
            var responseDTO = new ApplicationDTO
            {
                ApplicationId = application.ApplicationId,
                Name = application.Name,
                Description = application.Description
            };
            return new DataObjectResponse(true, "Tạo ứng dụng thành công", responseDTO);
        }

        // Plan methods
        public async Task<List<ApplicationPlanDTO>> GetPlansByApplicationIdAsync(int appId)
        {

            var result = await _context.ApplicationPlans.Where(p => p.ApplicationId == appId).ToListAsync();
            var responseDTO = result.Select(p => new ApplicationPlanDTO
            {
                ApplicationId = p.ApplicationId,
                Name = p.Name,
                Description = p.Description,
                PriceMonthly = p.PriceMonthly,
                PriceYearly = p.PriceYearly,
                MaxEmployees = p.MaxEmployees
            });
            return responseDTO.ToList();
        }

        public async Task<DataObjectResponse> CreatePlanAsync(ApplicationPlanDTO plan)
        {
            var applicationPlan = new ApplicationPlan
            {
                Name = plan.Name,
                Description = plan.Description,
                PriceMonthly = plan.PriceMonthly,
                PriceYearly = plan.PriceYearly,
                MaxEmployees = plan.MaxEmployees
            };
            _context.ApplicationPlans.Add(applicationPlan);
            await _context.SaveChangesAsync();

            var responseDTO = new ApplicationPlanDTO
            {
                ApplicationId = applicationPlan.ApplicationId,
                Name = applicationPlan.Name,
                Description = applicationPlan.Description,
                PriceMonthly = applicationPlan.PriceMonthly,
                PriceYearly = applicationPlan.PriceYearly,
                MaxEmployees = applicationPlan.MaxEmployees
            };
            return new DataObjectResponse(true, "Tạo gói dịch vụ thành công", responseDTO);
        }


        // License methods
        public async Task<List<PartnerLicenseDTO>> GetPartnerLicensesAsync(int partnerId)
        {

            var result = await _context.PartnerLicenses.Where(p => p.PartnerId == partnerId).ToListAsync();
            var responseDTO = result.Select(p => new PartnerLicenseDTO
            {
                PartnerId = p.PartnerId,
                ApplicationId = p.ApplicationId,
                ApplicationPlanId = p.ApplicationPlanId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                LicenceType = p.LicenceType,
                Status = p.Status
            }).ToList();
            return responseDTO;
        }

        public async Task<DataObjectResponse> CreateLicenseAsync(PartnerLicenseDTO license)
        {
            var partnerLicense = new PartnerLicense
            {
                PartnerId = license.PartnerId,
                ApplicationId = license.ApplicationId,
                ApplicationPlanId = license.ApplicationPlanId,
                StartDate = license.StartDate,
                EndDate = license.EndDate,
                LicenceType = license.LicenceType,
                Status = license.Status,
                MaxEmployeesExpected = license.MaxEmployeesExpected,
                CustomPrice = license.CustomPrice,
                AutoRenew = license.AutoRenew
            };
            _context.PartnerLicenses.Add(partnerLicense);
            await _context.SaveChangesAsync();
            return new DataObjectResponse(true, "Tạo giấy phép thành công", partnerLicense);
        }

        public async Task<DataObjectResponse> RenewLicenseAsync(int licenseId, int durationDays)
        {
            var license = await _context.PartnerLicenses.FindAsync(licenseId);
            if (license == null) throw new Exception("License not found");

            var now = DateTime.UtcNow;
            var newLicense = new PartnerLicense
            {
                PartnerId = license.PartnerId,
                ApplicationId = license.ApplicationId,
                ApplicationPlanId = license.ApplicationPlanId,
                StartDate = now,
                EndDate = now.AddDays(durationDays),
                LicenceType = license.LicenceType,
                Status = "Active",
                AutoRenew = license.AutoRenew,
                CustomPrice = license.CustomPrice,
                MaxEmployeesExpected = license.MaxEmployeesExpected
            };

            license.Status = "Expired";
            _context.PartnerLicenses.Add(newLicense);
            await _context.SaveChangesAsync();
            return new DataObjectResponse(true, "làm mới giấy phép thành công", newLicense);
        }

        public async Task<bool> ExpireLicenseAsync(int licenseId)
        {
            var license = await _context.PartnerLicenses.FindAsync(licenseId);
            if (license == null) return false;
            license.Status = "Expired";
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsLicenseExpiredAsync(int partnerId, int applicationId)
        {
            var license = await _context.PartnerLicenses
                .Where(l => l.PartnerId == partnerId && l.ApplicationId == applicationId && l.Status == "Active")
                .OrderByDescending(l => l.EndDate)
                .FirstOrDefaultAsync();

            if (license == null) // License expired
            {
                return true;
            }

            return license.EndDate < DateTime.UtcNow;
        }


        public async Task<List<UserLicenseDto>> GetUsersWithLicensesAsync(int partnerId)
        {
            var partnerUsers = await _context.PartnerUsers
                .Where(pu => pu.Partner.Id == partnerId)
                .Include(pu => pu.User)

                .Select(pu => new UserLicenseDto
                {
                    UserId = pu.User.Id,
                    FullName = pu.User.FullName ?? "Unknown",
                    Email = pu.User.Email ?? "N/A",
                    EmployeeId = pu.EmployeeId
                })
                .ToListAsync();

            var licenses = await _context.PartnerLicenses
                .Include(l => l.Application)
                .Include(l => l.ApplicationPlan)
                .Where(l => l.PartnerId == partnerId
                         && l.Status == "Active"
                         && l.StartDate <= DateTime.UtcNow
                         && l.EndDate >= DateTime.UtcNow)
                .Select(l => new LicenseForUserDTO
                {
                    ApplicationId = l.ApplicationId,
                    ApplicationName = l.Application.Name ?? "Unknown",
                    ApplicationPlanId = l.ApplicationPlanId,
                    PlanName = l.ApplicationPlan != null ? l.ApplicationPlan.Name : "N/A",
                    LicenceType = l.LicenceType ?? "N/A",
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    MaxEmployeesExpected = l.MaxEmployeesExpected
                })
                .ToListAsync();

            foreach (var user in partnerUsers)
            {
                user.Licenses = licenses;
            }

            return partnerUsers;
        }

        public async Task<UserLicenseDto> GetUserLicenseDetailsAsync(int userId, int? applicationId = null)
        {
            var partnerUser = await _context.PartnerUsers
                .Include(pu => pu.User)
                .Include(pu => pu.Partner)
                .FirstOrDefaultAsync(pu => pu.User.Id == userId);

            if (partnerUser == null)
                throw new KeyNotFoundException("User not found or not associated with a partner.");

            var userDto = new UserLicenseDto
            {
                UserId = partnerUser.User.Id,
                FullName = partnerUser.User.FullName ?? "Unknown",
                Email = partnerUser.User.Email ?? "N/A",
                EmployeeId = partnerUser.EmployeeId
            };

            var licenseQuery = _context.PartnerLicenses
                .Include(l => l.Application)
                .Include(l => l.ApplicationPlan)
                .Where(l => l.PartnerId == partnerUser.Partner.Id
                         && l.Status == "Active"
                         && l.StartDate <= DateTime.UtcNow
                         && l.EndDate >= DateTime.UtcNow);

            if (applicationId.HasValue)
            {
                licenseQuery = licenseQuery.Where(l => l.ApplicationId == applicationId.Value);
            }

            var licenses = await licenseQuery
                .Select(l => new LicenseForUserDTO
                {
                    ApplicationId = l.ApplicationId,
                    ApplicationName = l.Application.Name ?? "Unknown",
                    ApplicationPlanId = l.ApplicationPlanId,
                    PlanName = l.ApplicationPlan != null ? l.ApplicationPlan.Name : "N/A",
                    LicenceType = l.LicenceType ?? "N/A",
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    MaxEmployeesExpected = l.MaxEmployeesExpected
                })
                .ToListAsync();

            userDto.Licenses = licenses;
            return userDto;
        }

        public Task<bool> ValidLicenseAsync(int appId, ClaimsPrincipal user, List<AppClaim>? cachedApps = null)
        {
          var apps = cachedApps ?? JsonSerializer.Deserialize<List<AppClaim>>(user.FindFirst("Apps")?.Value ?? "");
            if (apps == null || !apps.Any())
                return Task.FromResult(false);

            var app = apps.FirstOrDefault(a => a.AppId == appId.ToString());
            if (app == null)
                return Task.FromResult(false);

            if (!DateTime.TryParse(app.AppExpiredAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiredAt) || expiredAt < DateTime.UtcNow)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
    }
}