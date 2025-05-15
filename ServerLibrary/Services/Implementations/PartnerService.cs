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

        public PartnerService(AppDbContext _appDbContext, ILogger<PartnerService> logger)
        {
            appDbContext = _appDbContext;
            // _jobGroupService = jobGroupService;
            logger = logger;
        }

        public async Task<DataObjectResponse> CreatePartnerFreeTrialAsync(CreatePartner partner)
        {
            var newPartner = await CreatePartnerAsync(partner);
            try
            {
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
                        Status = "Active",
                        CreatedAt = now,
                    });
                }
                await appDbContext.SaveChangesAsync();
                logger?.LogInformation("Partner {Name} created successfully with ID: {Id}", newPartner.Name, newPartner.Id);
                return new DataObjectResponse(true, "Đăng ký bản dùng thử thành công", newPartner);
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

        public async Task<Partner> CreatePartnerAsync(CreatePartner partner)
        {
            var partnerExist = await FindPartnerByEmail(partner.EmailContact);
            if (partnerExist != null)
            {
                logger?.LogWarning("Attempt to create partner with existing email: {Email}");
                throw new Exception("Email tạo doanh nghiệp đã tồn tại, vui lòng thử email khác");
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
                logger?.LogInformation("Partner {Name} created successfully with ID: {Id}", newPartner.Name, newPartner.Id);
                return newPartner;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to create partner with email: {Email}", null);
                throw new Exception("Đã xảy ra lỗi khi tạo doanh nghiệp");
            }
        }

        public async Task<bool?> FindUserOfPartner(int userId)
        {
            if (userId <= 0)
            {
                return null; // ID không hợp lệ
            }

            return await appDbContext.PartnerUsers
                .AnyAsync(pu => pu.User.Id == userId);
        }
    }
}
