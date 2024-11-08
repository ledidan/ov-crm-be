using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ProductService
        (AppDbContext appDbContext,
        IProductCatelogyService productCatelogyService) : IProductService
    {
        public async Task<GeneralResponse> CreateAsync(CreateProduct product, Partner partner)
        {
            try
            {
                //check Product Catelogy
                var productCatelogy = await productCatelogyService.FindById(product.ProductCatelogyId);
                if (productCatelogy == null) return new GeneralResponse(false, "Product Catelogy not found");

                //check Code Product existing
                var productChecking = await FindCodeByPartner(product.Code, partner);
                if (productChecking != null) return new GeneralResponse(false, "Code product existing");

                var productCreating = await appDbContext.InsertIntoDb(new Product()
                {
                    Code = product.Code,
                    Name = product.Name,
                    Unit = product.Unit,
                    ProducerName = product.ProducerName,
                    WarrantyPeriodPerMonth = product.WarrantyPeriodPerMonth,
                    ProductCatelogy = productCatelogy,
                    Partner = partner
                });

                await appDbContext.InsertIntoDb(new ProductPrice()
                {
                    Product = productCreating,
                    Price = product.SellingPrice,
                    IsLatest = true,
                    StartDate = DateTime.Now
                });

                return new GeneralResponse(true, "Product created");
            }
            catch
            {
                return new GeneralResponse(false, "Error!");
            }
        }

        private async Task<Product?> FindCodeByPartner(string code, Partner partner)
        {
            return await appDbContext.Products.FirstOrDefaultAsync(_ => _.Partner.Id == partner.Id
                                                                    && _.Code == code);
        }

        public async Task<List<Product>> GetAllAsync(Partner partner)
        {
            var result = await appDbContext.Products.Where(_ => _.Partner.Id == partner.Id).ToListAsync();
            return result;
        }

        public async Task<GeneralResponse> UpdateAsync(Product product)
        {
            //check Code Product
            var productChecking = await FindCodeByPartner(product.Code, product.Partner);
            if (productChecking != null) return new GeneralResponse(false, "Code product existing");

            await appDbContext.UpdateDb(product);
            return new GeneralResponse(true, "Product updated");
        }

        public async Task<GeneralResponse> UpdateSellingPriceAsync(Product product, double sellingPrice)
        {
            try
            {
                var productLatestPrice = await FindLatestPrice(product.Id);

                productLatestPrice.IsLatest = false;
                productLatestPrice.EndDate = DateTime.Now;

                await appDbContext.UpdateDb(productLatestPrice);

                await appDbContext.InsertIntoDb(new ProductPrice()
                {
                    Product = product,
                    Price = sellingPrice,
                    IsLatest = true,
                    StartDate = productLatestPrice.EndDate.AddSeconds(1)
                });

                return new GeneralResponse(true, "Selling Price updated");
            }
            catch
            {
                return new GeneralResponse(false, "Error!");
            }
        }

        private async Task<ProductPrice> FindLatestPrice(int productId)
        {
            return await appDbContext.ProductPrices.FirstAsync(_ => _.Product.Id == productId && _.IsLatest);
        }
    }
}
