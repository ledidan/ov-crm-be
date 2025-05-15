
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Data.Applications;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;
namespace ServerLibrary.Services.Implementations
{

    public class LicenseCenterService : ILicenseCenterService
    {

        private readonly AppDbContext _context;
        private readonly ILogger<LicenseCenterService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        public LicenseCenterService(AppDbContext context, IUserService userService, ILogger<LicenseCenterService> logger,
        IConfiguration configuration,
        IEmailService emailService
        )
        {

            _context = context;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
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
                ApplicationId = plan.ApplicationId,
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
            try
            {
                if (partnerId <= 0)
                {
                    _logger.LogWarning("PartnerId {PartnerId} không hợp lệ", partnerId);
                    return new List<PartnerLicenseDTO>();
                }
                var result = await _context.PartnerLicenses
          .Where(p => p.PartnerId == partnerId)
          .ToListAsync();
                var responseDTO = result
            .Where(p => p != null) // Loại bỏ phần tử null (nếu có)
            .Select(p => new PartnerLicenseDTO
            {
                PartnerId = p.PartnerId ?? 0,
                UserId = p.UserId, // Xử lý UserId null
                ApplicationId = p.ApplicationId,
                ApplicationPlanId = p.ApplicationPlanId,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                LicenceType = p.LicenceType ?? string.Empty, // Xử lý null
                Status = p.Status ?? string.Empty // Xử lý null
            })
            .ToList();
                _logger.LogInformation("Lấy thành công {Count} licenses cho partnerId {PartnerId}", responseDTO.Count, partnerId);
                return responseDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy giấy phép cho partner {PartnerId}", partnerId);
                throw;
            }
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
            // Đánh dấu là hết hạn ngay bây giờ
            license.EndDate = DateTime.UtcNow;
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

        public async Task<UserLicenseDto> GetLicenseDetailsByUserAsync(int userId, int partnerId)
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

            if (partnerId != null)
            {
                licenseQuery = licenseQuery.Where(l => l.PartnerId == partnerId);
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
            var apps = cachedApps ?? JsonConvert.DeserializeObject<List<AppClaim>>(user.FindFirst("Apps")?.Value ?? "");
            if (apps == null || !apps.Any())
                return Task.FromResult(false);

            var app = apps.FirstOrDefault(a => a.AppId == appId.ToString());
            if (app == null)
                return Task.FromResult(false);

            if (!DateTime.TryParse(app.AppExpiredAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiredAt) || expiredAt < DateTime.UtcNow)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }


        public async Task<List<PartnerLicense>> GetOrCreateLicenses(AppPaymentRequest request)
        {
            var licenses = new List<PartnerLicense>();

            foreach (var item in request.AppItems)
            {
                PartnerLicense license;
                if (request.PartnerLicenseId.HasValue && request.PartnerLicenseId.Value != 0)
                {
                    license = await _context.PartnerLicenses
                       .Include(l => l.ApplicationPlan)
                       .FirstOrDefaultAsync(l => l.Id == request.PartnerLicenseId && l.UserId == request.UserId);

                    if (license == null)
                    {
                        _logger.LogWarning("CreatePaymentForExistedLicense: Không tìm thấy PartnerLicense {PartnerLicenseId} cho User {UserId} và ApplicationPlan {ApplicationPlanId}", request.PartnerLicenseId, request.UserId, item.ApplicationPlanId);
                        throw new Exception("License không hợp lệ hoặc không thuộc user này!");
                    }
                }
                else
                {
                    var plan = await _context.ApplicationPlans
                        .FirstOrDefaultAsync(p => p.Id == item.ApplicationPlanId);
                    if (plan == null)
                    {
                        _logger.LogWarning("CreatePaymentForExistedLicense: Không tìm thấy ApplicationPlan {ApplicationPlanId}", item.ApplicationPlanId);
                        throw new Exception($"Plan {item.ApplicationPlanId} không tồn tại, chọn lại nha!");
                    }
                    // ** Tạo luôn license khi chưa có Partner.
                    license = new PartnerLicense
                    {
                        UserId = request.UserId,
                        ApplicationId = plan.ApplicationId,
                        ApplicationPlanId = item.ApplicationPlanId,
                        LicenceType = item.LicenceType,
                        Status = "Pending",
                        StartDate = DateTime.UtcNow,
                        EndDate = item.LicenceType == "Monthly" ? DateTime.UtcNow.AddMonths(item.Duration ?? 1) :
                                  item.LicenceType == "Yearly" ? DateTime.UtcNow.AddYears(item.Duration ?? 1) :
                                  DateTime.UtcNow.AddYears(1),
                        AutoRenew = false
                    };
                    _context.PartnerLicenses.Add(license);
                }
                licenses.Add(license);
            }

            await _context.SaveChangesAsync();
            return licenses;
        }

        public async Task<DataObjectResponse> ActivateLicense(ActivateLicenseRequest request)
        {
            try
            {
                // Check user trước, không đổi
                var user = await _context.ApplicationUsers.Where(u => u.Email == request.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    _logger.LogWarning("Không thấy user với email {Email}", request.Email);
                    return new DataObjectResponse(false, "Không tìm thấy user với email này");
                }

                // Validate mã kích hoạt
                if (string.IsNullOrWhiteSpace(request.Code))
                {
                    _logger.LogWarning("Mã kích hoạt rỗng");
                    return new DataObjectResponse(false, "Mã kích hoạt không hợp lệ");
                }

                var license = await _context.PartnerLicenses
             .Where(l => l.ActivationCode == request.Code && l.UserId == user.Id && request.ApplicationId == l.ApplicationId)
             .FirstOrDefaultAsync();

                if (license == null)
                {
                    _logger.LogWarning("Không tìm thấy license với mã {Code} cho user {UserId}", request.Code, user.Id);
                    return new DataObjectResponse(false, "Mã kích hoạt không chính xác hoặc không khớp với user.");
                }
                // Kiểm tra license
                if (license.Status == "Pending")
                {
                    license.Status = "Active";
                    license.ActivationCode = null;
                    _context.PartnerLicenses.Update(license);
                    _logger.LogInformation("License {LicenseId} đã Active cho user {UserId}", license.Id, user.Id);
                }
                else if (license.Status == "Active")
                {
                    _logger.LogWarning("License {LicenseId} đã Active.", license.Id);
                    return new DataObjectResponse(true, "License đã được kích hoạt trước đó!");
                }
                else
                {
                    _logger.LogWarning("License {LicenseId} có trạng thái {Status}, không thể kích hoạt!", license.Id, license.Status);
                    return new DataObjectResponse(false, "License không thể kích hoạt do trạng thái không hợp lệ");
                }

                // Active account user
                user.AccountStatus = AccountStatus.Verified;
                user.IsActivateEmail = true;
                user.IsActive = true;
                await _context.UpdateDb(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Kích hoạt thành công với mã {Code} cho user {UserId}", request.Code, user.Id);

                return new DataObjectResponse(true, "Kích hoạt license thành công !");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kích hoạt license với request {@Request}", request);
                return new DataObjectResponse(false, "Lỗi hệ thống, thử lại sau nha!");
            }
        }

        public async Task<DataObjectResponse> AssignDefaultApplicationLicenseToPartner(int partnerId)
        {
            var now = DateTime.UtcNow;
            var defaultApps = await _context.Applications
                .ToListAsync();

            foreach (var app in defaultApps)
            {
                await _context.AddAsync(new PartnerLicenseDTO()
                {
                    PartnerId = partnerId,
                    ApplicationId = app.ApplicationId,
                    StartDate = now,
                    EndDate = now.AddDays(1),
                    LicenceType = "Monthly",
                    Status = "Active"
                });
            }
            await _context.SaveChangesAsync();

            return new DataObjectResponse(true, "Cấp giấy phép ứng dụng mặc định cho đối tác thành công", null);
        }

        public async Task<bool?> IsLicenseActiveAsync(int userId)
        {
            return await _context.PartnerLicenses
                .AnyAsync(l => l.UserId == userId &&
                l.Status == "Active"
                && l.PartnerId != null);
        }

        public async Task SendActivationEmailAsync(string email, string fullName, string activationCode, string appName)
        {
            try
            {
                // Validate configuration
                var baseUrl = _configuration["Frontend:BaseUrl"];
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("Frontend:BaseUrl is not configured.");
                }

                // Construct activation link
                var activationLink = $"{baseUrl}/activate?code={Uri.EscapeDataString(activationCode)}";

                // Create the model for the email template
                var model = new ActivationEmailModel
                {
                    FullName = fullName,
                    VerificationLink = activationLink,
                    Email = email,
                    ActivationCode = activationCode,
                    AppName = appName
                };

                var templateName = "ActivationEmail.cshtml";
                var emailBody = await _emailService.GetActivateEmailTemplateAsync(model, templateName);

                await _emailService.SendEmailAsync(email, $"Kích hoạt tài khoản {appName}", emailBody);

                _logger.LogInformation($"Activation email sent to {email}");
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"Failed to send activation email to {email}");
                throw new Exception($"Failed to send activation email to {email}", ex);
            }
        }

        public async Task<DataObjectResponse> UpdateApplicationAsync(ApplicationDTO app)
        {
            var existing = await _context.Applications.FindAsync(app.ApplicationId);

            if (existing == null)
            {
                return new DataObjectResponse(false, "Ứng dụng không tồn tại");
            }
            existing.Name = app.Name;
            existing.Description = app.Description;
            await   _context.UpdateDb(existing);
            return new DataObjectResponse(true, "Cập nhật ứng dụng thành công", existing);
        }

        public async Task<DataObjectResponse> UpdatePlanAsync(ApplicationPlanDTO plan)
        {
            var existing = _context.ApplicationPlans.Find(plan.Id);
            if (existing == null)
            {
                return new DataObjectResponse(false, "Gói dịch vụ không tồn tại");
            }
            existing.Name = plan.Name;
            existing.Description = plan.Description;
            existing.PriceMonthly = plan.PriceMonthly;
            existing.PriceYearly = plan.PriceYearly;
            existing.MaxEmployees = plan.MaxEmployees;
            _context.Update(existing);
            await _context.SaveChangesAsync();
            return new DataObjectResponse(true, "Cập nhật gói dịch vụ thành công", existing);
        }
    }
}