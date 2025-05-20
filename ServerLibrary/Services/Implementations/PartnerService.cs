using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using AutoMapper;
namespace ServerLibrary.Services.Implementations
{
    public class PartnerService : IPartnerService
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<PartnerService> logger;
        private readonly IMapper _mapper;

        public PartnerService(AppDbContext _appDbContext, ILogger<PartnerService> logger, IMapper mapper)
        {
            appDbContext = _appDbContext;
            logger = logger;
            _mapper = mapper;
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

        public async Task<PartnerDTO> GetPartnerInfoAsync(int partnerId)
        {
            var result = await appDbContext.Partners.FirstOrDefaultAsync(_ => _.Id == partnerId);
            var response = new PartnerDTO
            {
                Id = result.Id,
                Name = result.Name,
                TotalEmployees = result.TotalEmployees,
                IsOrganization = result.IsOrganization,
                OwnerFullName = result.OwnerFullName,
                LogoUrl = result.LogoUrl,
                IsInitialized = result.IsInitialized,
                ShortName = result.ShortName,
            };
            return response;
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

        public async Task<Partner> UpdatePartnerAsync(PartnerDTO model)
        {
            var existingPartner = await appDbContext.Partners.FindAsync(model.Id);
            if (existingPartner == null)
            {
                logger?.LogWarning("Attempt to update non-existent partner with ID: {Id}", model.Id);
                throw new Exception("Không tìm thấy doanh nghiệp để cập nhật");
            }

            if (!string.IsNullOrEmpty(model.EmailContact) && model.EmailContact != existingPartner.EmailContact)
            {
                var emailUsed = await appDbContext.Partners.AnyAsync(p => p.EmailContact == model.EmailContact && p.Id != model.Id);
                if (emailUsed)
                {
                    throw new Exception("Email đã được sử dụng bởi doanh nghiệp khác");
                }
            }
            try
            {
                existingPartner.ShortName = model.ShortName;
                existingPartner.Name = model.Name;
                existingPartner.TaxIdentificationNumber = model.TaxIdentificationNumber;
                existingPartner.LogoUrl = model.LogoUrl;
                existingPartner.EmailContact = model.EmailContact;
                existingPartner.TotalEmployees = model.TotalEmployees;
                existingPartner.IsOrganization = model.IsOrganization;
                existingPartner.OwnerFullName = model.OwnerFullName;
                existingPartner.PhoneNumber = model.PhoneNumber;
                existingPartner.ModifiedDate = DateTime.UtcNow;

                appDbContext.Partners.Update(existingPartner);
                await appDbContext.SaveChangesAsync();

                logger?.LogInformation("Partner {Name} (ID: {Id}) updated successfully", existingPartner.Name, existingPartner.Id);
                return existingPartner;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error updating partner with ID: {Id}", model.Id);
                throw new Exception("Đã xảy ra lỗi khi cập nhật doanh nghiệp");
            }
        }

    }
}
