using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;
using System.Security.Cryptography.X509Certificates;

namespace ServerLibrary.Services.Implementations
{
    public class PartnerService(AppDbContext appDbContext) : IPartnerService
    {
        public async Task<GeneralResponse> CreateAsync(CreatePartner partner)
        {
            if (partner == null) return new GeneralResponse(false, "Model is empty");

            var checkingPartner = await FindPartnerByShortName(partner.ShortName);
            if (checkingPartner != null) return new GeneralResponse(false, "Partner existing");

            await appDbContext.AddToDatabase(new Partner()
            {
                ShortName = partner.ShortName,
                Name = partner.Name,
                TaxIdentificationNumber = partner.TaxIdentificationNumber,
                LogoUrl = partner.LogoUrl,
                EmailContact = partner.EmailContact,
                PhoneNumber = partner.PhoneNumber
            });
            return new GeneralResponse(true, "Partner created");
        }

        private async Task<Partner?> FindPartnerByShortName(string shortName)
        {
            return await appDbContext.Partners.FirstOrDefaultAsync(_ => _.ShortName == shortName);
        }

        public async Task<Partner?> FindById(int id)
        {
            return await appDbContext.Partners.FirstOrDefaultAsync(_ => _.Id == id);
        }

        public async Task<List<Partner>> GetAsync()
        {
            return await appDbContext.Partners.ToListAsync();
        }
    }
}
