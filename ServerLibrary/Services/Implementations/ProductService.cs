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
        IProductCategoryService productCategoryService) : IProductService
    {
        public async Task<GeneralResponse> CreateAsync(CreateProduct product, Employee employee, Partner partner)
        {
            try
            {
                //check Code Product existing
                var productChecking = await FindCodeByPartner(product.ProductCode, partner);
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
                    IsDeleted = false,
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

        private async Task<Product?> FindCodeByPartner(string ProductCode, Partner partner)
        {
            return await appDbContext.Products.FirstOrDefaultAsync(_ => _.Partner.Id == partner.Id
                                                                    && _.ProductCode == ProductCode);
        }

        public async Task<List<ProductDTO>> GetAllAsync(Employee employee, Partner partner)
        {
            var products = await appDbContext.Products
                .Where(p => p.Partner.Id == partner.Id)
                .ToListAsync();

            // Convert to DTO
            var productDTOs = products.Select(p => new ProductDTO
            {
                Id = p.Id,
                ProductCode = p.ProductCode,
                ProductGroupID = p.ProductGroupID,
                ProductGroupName = p.ProductGroupName,
                ProductName = p.ProductName,
                AmountSummary = p.AmountSummary,
                Avatar = p.Avatar,
                ConversionRate = p.ConversionRate,
                ConversionUnit = p.ConversionUnit,
                CreatedBy = p.CreatedBy,
                CustomID = p.CustomID,
                Description = p.Description,
                Equation = p.Equation,
                Inactive = p.Inactive,
                InventoryItemID = p.InventoryItemID,
                IsDeleted = p.IsDeleted,
                IsFollowSerialNumber = p.IsFollowSerialNumber,
                IsPublic = p.IsPublic,
                IsSetProduct = p.IsSetProduct,
                IsSystem = p.IsSystem,
                IsUseTax = p.IsUseTax,
                ModifiedBy = p.ModifiedBy,
                OldProductCode = p.OldProductCode,
                OperatorID = p.OperatorID,
                PriceAfterTax = p.PriceAfterTax,
                ProductPropertiesID = p.ProductPropertiesID,
                PurchasedPrice = p.PurchasedPrice,
                QuantityDemanded = p.QuantityDemanded,
                QuantityFormula = p.QuantityFormula,
                QuantityInstock = p.QuantityInstock,
                QuantityOrdered = p.QuantityOrdered,
                SaleDescription = p.SaleDescription,
                SearchTagID = p.SearchTagID,
                TagColor = p.TagColor,
                TagID = p.TagID,
                TaxID = p.TaxID,
                Taxable = p.Taxable,
                UnitCost = p.UnitCost,
                UnitPrice = p.UnitPrice,
                UnitPrice1 = p.UnitPrice1,
                UnitPrice2 = p.UnitPrice2,
                UnitPriceFixed = p.UnitPriceFixed,
                UsageUnitID = p.UsageUnitID,
                VendorNameID = p.VendorNameID,
                WarrantyDescription = p.WarrantyDescription,
                WarrantyPeriod = p.WarrantyPeriod,
                WarrantyPeriodTypeID = p.WarrantyPeriodTypeID,
                OwnerID = p.OwnerID,
                ProductCategoryId = p.ProductCategory?.Id // Ensure the category exists
            }).ToList();

            return productDTOs;
        }


        // public async Task<GeneralResponse> UpdateAsync(Product product)
        // {
        //     //check Code Product
        //     var productChecking = await FindCodeByPartner(product.Code, product.Partner);
        //     if (productChecking != null) return new GeneralResponse(false, "Code product existing");

        //     await appDbContext.UpdateDb(product);
        //     return new GeneralResponse(true, "Product updated");
        // }

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
    }
}
