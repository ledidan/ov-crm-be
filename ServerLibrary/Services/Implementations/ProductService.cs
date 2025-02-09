using AutoMapper;
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
        IProductCategoryService productCategoryService, IMapper _mapper) : IProductService
    {
        public async Task<GeneralResponse> CreateAsync(CreateProductDTO product, Employee employee, Partner partner)
        {
            try
            {
                //check Code Product existing
                var productChecking = await FindCodeByPartner(product.ProductCode, partner.Id);
                if (productChecking != null) return new GeneralResponse(false, "Code product existing");

                var productCategory = await appDbContext.ProductCategories
                    .FirstOrDefaultAsync(pc => pc.Id == product.ProductCategoryId);

                var productCreating = new Product()
                {
                    ProductCode = product.ProductCode,
                    ProductGroupID = product.ProductGroupID ?? "",
                    ProductGroupName = product.ProductGroupName ?? "",
                    ProductName = product.ProductName,
                    AmountSummary = product.AmountSummary ?? 0,
                    Avatar = product.Avatar ?? "",
                    ConversionRate = product.ConversionRate ?? 0,
                    ConversionUnit = product.ConversionUnit ?? "",
                    CreatedBy = employee.Fullname,
                    CustomID = product.CustomID ?? "",
                    Description = product.Description ?? "",
                    Equation = product.Equation ?? "",
                    Inactive = product.Inactive ?? false,
                    InventoryItemID = product.InventoryItemID ?? "",
                    IsFollowSerialNumber = product.IsFollowSerialNumber ?? false,
                    IsPublic = product.IsPublic ?? false,
                    IsSetProduct = product.IsSetProduct ?? false,
                    IsSystem = product.IsSystem ?? false,
                    IsUseTax = product.IsUseTax ?? false,
                    ModifiedBy = employee.Id.ToString(),
                    OldProductCode = product.OldProductCode ?? "",
                    OperatorID = product.OperatorID ?? "",
                    PriceAfterTax = product.PriceAfterTax ?? false,
                    ProductPropertiesID = product.ProductPropertiesID ?? "",
                    PurchasedPrice = product.PurchasedPrice ?? 0,
                    QuantityDemanded = product.QuantityDemanded ?? 0,
                    QuantityFormula = product.QuantityFormula ?? "",
                    QuantityInstock = product.QuantityInstock ?? 0,
                    QuantityOrdered = product.QuantityOrdered ?? 0,
                    SaleDescription = product.SaleDescription ?? "",
                    SearchTagID = product.SearchTagID ?? "",
                    TagColor = product.TagColor ?? "",
                    TagID = product.TagID ?? "",
                    TaxID = product.TaxID ?? "",
                    Taxable = product.Taxable ?? false,
                    UnitCost = product.UnitCost ?? 0,
                    UnitPrice = product.UnitPrice ?? 0,
                    UnitPrice1 = product.UnitPrice1 ?? 0,
                    UnitPrice2 = product.UnitPrice2 ?? 0,
                    UnitPriceFixed = product.UnitPriceFixed ?? 0,
                    UsageUnitID = product.UsageUnitID ?? "",
                    VendorNameID = product.VendorNameID ?? "",
                    WarrantyDescription = product.WarrantyDescription ?? "",
                    WarrantyPeriod = product.WarrantyPeriod ?? "",
                    WarrantyPeriodTypeID = product.WarrantyPeriodTypeID ?? "",
                    OwnerID = employee.Id,
                    ProductCategory = productCategory,
                    Partner = partner
                };

                await appDbContext.InsertIntoDb(productCreating);

                await appDbContext.InsertIntoDb(new ProductPrice()
                {
                    Product = productCreating,
                    Price = product.UnitPrice ?? 0,
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

        private async Task<Product?> FindCodeByPartner(string ProductCode, int partnerId)
        {
            return await appDbContext.Products.FirstOrDefaultAsync(_ => _.Partner.Id == partnerId
                                                                    && _.ProductCode == ProductCode);
        }

        public async Task<List<ProductDTO>> GetAllAsync(Employee employee, Partner partner)
        {
            if (partner == null) return new List<ProductDTO>();

            var products = await appDbContext.Products
            .Where(p => p.Partner.Id == partner.Id)
            .ToListAsync();

            return _mapper.Map<List<ProductDTO>>(products);
        }


        public async Task<GeneralResponse> UpdateAsync(int id, UpdateProductDTO product, Partner partner)
        {
            // Check if product exists
            if (partner == null) return new GeneralResponse(false, "Invalid partner");
            var existingProduct = await appDbContext.Products
        .Where(p => p.Id == id && p.Partner.Id == partner.Id)

        .FirstOrDefaultAsync();
            if (existingProduct == null) return new GeneralResponse(false, "Product not found");

            //check Code Product

            var productChecking = await FindCodeByPartner(product.ProductCode, partner.Id);
            if (productChecking != null && productChecking.Id != id)
                return new GeneralResponse(false, "Product code already exists");

            _mapper.Map(product, existingProduct);

            await appDbContext.UpdateDb(existingProduct);
            return new GeneralResponse(true, "Product updated successfully");
        }

        // public async Task<GeneralResponse> UpdateSellingPriceAsync(Product product, double sellingPrice)
        // {
        //     try
        //     {
        //         var productLatestPrice = await FindLatestPrice(product.Id);

        //         productLatestPrice.IsLatest = false;
        //         productLatestPrice.EndDate = DateTime.Now;

        //         await appDbContext.UpdateDb(productLatestPrice);

        //         await appDbContext.InsertIntoDb(new ProductPrice()
        //         {
        //             Product = product,
        //             Price = sellingPrice,
        //             IsLatest = true,
        //             StartDate = productLatestPrice.EndDate.AddSeconds(1)
        //         });

        //         return new GeneralResponse(true, "Selling Price updated");
        //     }
        //     catch
        //     {
        //         return new GeneralResponse(false, "Error!");
        //     }
        // }

        private async Task<ProductPrice> FindLatestPrice(int productId)
        {
            return await appDbContext.ProductPrices.FirstAsync(_ => _.Product.Id == productId && _.IsLatest);
        }

        public async Task<ProductDTO?> FindByIdAsync(int id, Partner partner)
        {
            if (partner == null) return null;

            var product = await appDbContext.Products.Where(p => p.Id == id && p.Partner.Id == partner.Id).FirstOrDefaultAsync();

            return product == null ? null : _mapper.Map<ProductDTO>(product);
        }

        public async Task<GeneralResponse> RemoveBulkIdsAsync(string ids, Partner partner)
        {
            if (partner == null) return new GeneralResponse(false, "Invalid partner");

            var idList = ids.Split(',')
                .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            if (!idList.Any()) return new GeneralResponse(false, "No valid IDs provided");

            var products = await appDbContext.Products
                .Where(p => idList.Contains(p.Id) && p.Partner.Id == partner.Id)
                .ToListAsync();

            if (!products.Any()) return new GeneralResponse(false, "No matching products found");

            appDbContext.Products.RemoveRange(products);
            await appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, $"{products.Count} product(s) marked as deleted");
        }
    }
}
