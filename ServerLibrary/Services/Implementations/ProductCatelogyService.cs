using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ProductCatelogyService(AppDbContext appDbContext) : IProductCatelogyService
    {
        public async Task<GeneralResponse> CreateAsync(CreateProductCatelogy productCatelogy, Partner partner)
        {
            if (productCatelogy == null) return new GeneralResponse(false, "Model is empty");
            
            await appDbContext.AddToDatabase(new ProductCatelogy()
            {
                Name = productCatelogy.Name,
                Description = productCatelogy.Description,
                Partner = partner
            });

            return new GeneralResponse(true, "Product Catelogy created");
        }

        public async Task<List<ProductCatelogy>> GetAllAsync(Partner partner)
        {
            if (partner == null) return new List<ProductCatelogy>();

            var result = await appDbContext.ProductCatelogies.Where(_ => _.Partner.Id == partner.Id).ToListAsync();

            return result;
        }

        public async Task<GeneralResponse> UpdateAsync(ProductCatelogy productCatelogy)
        {
            if (productCatelogy == null) return new GeneralResponse(false, "Model is empty");

            //check customer
            var productCatelogyUpdating = await appDbContext.ProductCatelogies.FirstOrDefaultAsync(_ => _.Id == productCatelogy.Id);
            if (productCatelogyUpdating == null) return new GeneralResponse(false, "Product Catelogy not found");

            appDbContext.ProductCatelogies.Update(productCatelogy);
            appDbContext.SaveChanges();
            return new GeneralResponse(true, "Customer updated successfully");
        }
    }
}
