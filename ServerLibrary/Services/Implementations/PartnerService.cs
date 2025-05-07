using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
namespace ServerLibrary.Services.Implementations
{
    public class PartnerService : IPartnerService
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<PartnerService> logger;
        private readonly IJobGroupService _jobGroupService;

        // !! Đéo bao giờ được thêm IUserService vào constructor PartnerService nha các bạn. Lỗi CircularityDependencyError 
        public PartnerService(AppDbContext _appDbContext, ILogger<PartnerService> logger)
        {
            appDbContext = _appDbContext;
            // _jobGroupService = jobGroupService;
            logger = logger;
        }
        public async Task<DataObjectResponse> CreateAsync(CreatePartner partner)
        {
            var partnerExist = await FindPartnerByEmail(partner.EmailContact);
            if (partnerExist != null)
            {
                logger?.LogWarning("Attempt to create partner with existing email: {Email}");
                return new DataObjectResponse(false, "Email tạo doanh nghiệp đã tồn tại, vui lòng thử email khác.", null);
            }
            var newPartner = new Partner
            {
                ShortName = partner.ShortName,
                Name = partner.Name,
                TaxIdentificationNumber = partner.TaxIdentificationNumber,
                LogoUrl = partner.LogoUrl,
                EmailContact = partner.EmailContact,
                TotalEmployees = partner.TotalEmployees,
                IsOrganization = partner.IsOrganization,
                OwnerFullName = partner.OwnerFullName,
                PhoneNumber = partner.PhoneNumber,
            };
            try
            {
                await appDbContext.InsertIntoDb(newPartner);
                // Thêm logic từ AssignDefaultApplicationsToPartner
                var now = DateTime.UtcNow;
                var defaultApps = await appDbContext.Applications.ToListAsync();
                foreach (var app in defaultApps)
                {
                    await appDbContext.AddAsync(new PartnerLicense
                    {
                        PartnerId = newPartner.Id,
                        ApplicationId = app.ApplicationId,
                        StartDate = now,
                        EndDate = now.AddDays(1),
                        LicenceType = "FreeTrial",
                        Status = "Active"
                    });
                }
                await appDbContext.SaveChangesAsync();
                logger?.LogInformation("Partner {Name} created successfully with ID: {Id}", newPartner.Name, newPartner.Id);
                return new DataObjectResponse(true, "Tạo doanh nghiệp thành công", newPartner);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to create partner with email: {Email}", null);
                return new DataObjectResponse(false, "Đã xảy ra lỗi khi tạo doanh nghiệp", null);
            }
        }

        private async Task<Partner?> FindPartnerByShortName(string shortName)
        {
            return await appDbContext.Partners.FirstOrDefaultAsync(_ => _.ShortName == shortName);
        }

        private async Task<Partner?> FindPartnerByEmail(string email)
        {
            return await appDbContext.Partners.FirstOrDefaultAsync(_ => _.EmailContact == email);
        }

        public async Task<Partner?> FindById(int id)
        {
            return await appDbContext.Partners.FirstOrDefaultAsync(_ => _.Id == id);
        }

        public async Task<List<Partner>> GetAsync()
        {
            return await appDbContext.Partners.ToListAsync();
        }

        public async Task<Partner?> FindByClaim(ClaimsIdentity? claimsIdentity)
        {
            try
            {
                var value = claimsIdentity?.FindFirst("PartnerId")?.Value;
                if (value == null) return default(Partner);

                int partnerId = Int32.Parse(value);
                var partner = await FindById(partnerId);
                return partner;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return default(Partner);
        }

        public async Task<bool?> CheckClaimByOwner(ClaimsIdentity? claimsIdentity)
        {
            try
            {
                var isOwner = claimsIdentity?.FindFirst("Owner")?.Value;
                if (isOwner == null) return false;
                if (isOwner == "true")
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return false;
        }
    }
}
