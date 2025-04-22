using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext appDbContext;
        private readonly IMongoCollection<OrderDetails> _ordersDetailsCollection;

        private readonly IMongoCollection<InvoiceDetails> _invoicesDetailsCollection;
        private readonly IProductCategoryService _productCategoryService;

        private readonly S3Service _s3Service;
        private readonly IMapper _mapper;

        public ProductService(
            AppDbContext _appDbContext,
            MongoDbContext dbContext,
            S3Service s3Service,
            IProductCategoryService productCategoryService,
            IMapper mapper
        )
        {
            _ordersDetailsCollection = dbContext.OrderDetails;
            _invoicesDetailsCollection = dbContext.InvoiceDetails;
            appDbContext = _appDbContext;
            _productCategoryService = productCategoryService;
            _mapper = mapper;
            _s3Service = s3Service;
        }

        public async Task<GeneralResponse> CreateAsync(
            CreateProductDTO product,
            Employee employee,
            Partner partner
        )
        {
            try
            {
                var codeGenerator = new GenerateNextCode(appDbContext);
                //check Code Product existing
                var productChecking = await FindCodeByPartner(product.ProductCode, partner.Id);
                if (productChecking != null)
                    return new GeneralResponse(false, "Mã hàng hoá đã tồn tại");

                var productCategory = await appDbContext.ProductCategories.FirstOrDefaultAsync(pc =>
                    pc.Id == product.ProductCategoryId
                );
                if (string.IsNullOrEmpty(product.ProductCode))
                {
                    product.ProductCode = await codeGenerator.GenerateNextCodeAsync<Product>(
                        "HH",
                        c => c.ProductCode,
                        c => c.Partner.Id == partner.Id
                    );
                }
                string? avatarUrl = null;
                if (
                    !string.IsNullOrEmpty(product.Avatar) && product.Avatar.StartsWith("data:image")
                )
                {
                    avatarUrl = await _s3Service.UploadBase64ImageToS3(
                        product.Avatar,
                        "product",
                        partner.Id,
                        employee.Id,
                        "crm",
                        "partner"
                    );
                }
                var productCreating = new Product()
                {
                    ProductCode = product.ProductCode,
                    ProductGroupID = product.ProductGroupID ?? "",
                    ProductGroupName = product.ProductGroupName ?? "",
                    ProductName = product.ProductName,
                    AmountSummary = product.AmountSummary ?? 0,
                    Avatar = avatarUrl ?? "",
                    ConversionRate = product.ConversionRate ?? 0,
                    ConversionUnit = product.ConversionUnit ?? "",
                    CreatedBy = employee.FullName,
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
                    OwnerIDName = employee.FullName,
                    ProductCategory = productCategory,
                    Partner = partner,
                };

                await appDbContext.InsertIntoDb(productCreating);

                await appDbContext.InsertIntoDb(
                    new ProductPrice()
                    {
                        Product = productCreating,
                        Price = product.UnitPrice ?? 0,
                        IsLatest = true,
                        StartDate = DateTime.Now,
                    }
                );

                return new GeneralResponse(true, "Đã tạo sản phẩm thành công");
            }
            catch
            {
                return new GeneralResponse(false, "Error!");
            }
        }

        private async Task<Product?> FindCodeByPartner(string ProductCode, int partnerId)
        {
            return await appDbContext.Products.FirstOrDefaultAsync(_ =>
                _.Partner.Id == partnerId && _.ProductCode == ProductCode
            );
        }

        public async Task<List<ProductDTO>> GetAllAsync(Employee employee, Partner partner)
        {
            if (partner == null)
                return new List<ProductDTO>();

            var products = await appDbContext
                .Products.Where(p => p.Partner.Id == partner.Id)
                .ToListAsync();

            return _mapper.Map<List<ProductDTO>>(products);
        }

        public async Task<GeneralResponse> UpdateAsync(
            int id,
            UpdateProductDTO product,
            Partner partner,
            Employee employee
        )
        {
            var codeGenerator = new GenerateNextCode(appDbContext);
            // Check if product exists
            if (partner == null)
                return new GeneralResponse(false, "Thông tin tổ chức không hợp lệ !");

            var existingProduct = await appDbContext
                .Products.Where(p => p.Id == id && p.Partner.Id == partner.Id)
                .FirstOrDefaultAsync();

            // ** Handle Avatar Product on S3 **
            string? avatarUrl = null;
            if (!string.IsNullOrEmpty(product.Avatar) && product.Avatar.StartsWith("data:image"))
            {
                Console.WriteLine("s3 service is working");
                avatarUrl = await _s3Service.UploadBase64ImageToS3(
                    product.Avatar,
                    "product",
                    partner.Id,
                    employee.Id,
                    "crm",
                    "partner"
                );
                Console.WriteLine("avatarUrl: " + avatarUrl);
            }
            else if (
                (
                    product.Avatar == null
                    || string.IsNullOrEmpty(product.Avatar)
                    || product.Avatar == "null"
                ) && !string.IsNullOrEmpty(existingProduct.Avatar)
            )
            {
                Console.WriteLine("Condition triggered: Removing existing avatar from S3");
                await _s3Service.RemoveFileFromS3(existingProduct.Avatar);
                avatarUrl = null;
            }
            if (existingProduct == null)
                return new GeneralResponse(false, "Không tim thấy hàng hoá");
            //check Code Product
            var productChecking = await FindCodeByPartner(product.ProductCode, partner.Id);
            if (productChecking != null && productChecking.Id != id)
                return new GeneralResponse(false, "Mã hàng hoá đã tồn tại !");

            _mapper.Map(product, existingProduct);

            if (string.IsNullOrEmpty(existingProduct.ProductCode))
            {
                existingProduct.ProductCode = await codeGenerator.GenerateNextCodeAsync<Product>(
                    "HH",
                    c => c.ProductCode,
                    c => c.Partner.Id == partner.Id
                );
            }
            existingProduct.Avatar = avatarUrl ?? existingProduct.Avatar;

            await appDbContext.UpdateDb(existingProduct);
            return new GeneralResponse(true, "Cập nhật hàng hoá thành công.");
        }

        public async Task<GeneralResponse?> UpdateFieldIdAsync(
            int id,
            UpdateProductDTO? product,
            Employee employee,
            Partner partner
        )
        {
            var strategy = appDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await appDbContext.Database.BeginTransactionAsync();
                try
                {
                    if (product == null)
                        return new GeneralResponse(
                            false,
                            "Dữ liệu sản phẩm (product DTO) không được để trống"
                        );

                    if (partner == null)
                        return new GeneralResponse(false, "Đối tác không hợp lệ");

                    if (employee == null)
                        return new GeneralResponse(false, "Nhân viên không hợp lệ");

                    var existingProduct = await appDbContext
                        .Products.Where(p =>
                            p.Id == id && p.Partner.Id == partner.Id && p.OwnerID == employee.Id
                        )
                        .FirstOrDefaultAsync();

                    if (product.Id == existingProduct?.Id)
                        return new GeneralResponse(
                            false,
                            "ID trong DTO không khớp với ID sản phẩm cần cập nhật"
                        );

                    if (existingProduct == null)
                        return new GeneralResponse(
                            false,
                            $"Không tìm thấy sản phẩm với ID {id} thuộc đối tác {partner.Id} và nhân viên {employee.Id}"
                        );

                    if (!string.IsNullOrEmpty(product.ProductCode))
                    {
                        var productChecking = await FindCodeByPartner(
                            product.ProductCode,
                            partner.Id
                        );
                        if (productChecking != null && productChecking.Id != id)
                            return new GeneralResponse(
                                false,
                                $"Mã sản phẩm '{product.ProductCode}' đã tồn tại cho đối tác {partner.Id}"
                            );
                    }

                    var properties = typeof(UpdateProductDTO).GetProperties();
                    foreach (var prop in properties)
                    {
                        var newValue = prop.GetValue(product);
                        if (newValue != null && newValue.ToString() != "")
                        {
                            var existingProp = typeof(Product).GetProperty(prop.Name);
                            if (existingProp != null && existingProp.CanWrite) // Kiểm tra thuộc tính có thể ghi
                            {
                                if (prop.Name != "Id")
                                {
                                    existingProp.SetValue(existingProduct, newValue);
                                    appDbContext
                                        .Entry(existingProduct)
                                        .Property(existingProp.Name)
                                        .IsModified = true;
                                }
                            }
                        }
                    }
                    await appDbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new GeneralResponse(true, "Cập nhật sản phẩm thành công");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(false, $"Cập nhật sản phẩm thất bại: {ex.Message}");
                }
            });
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
            return await appDbContext.ProductPrices.FirstAsync(_ =>
                _.Product.Id == productId && _.IsLatest
            );
        }

        public async Task<ProductDTO?> FindByIdAsync(int id, Partner partner)
        {
            if (partner == null)
                return null;

            var product = await appDbContext
                .Products.Where(p => p.Id == id && p.Partner.Id == partner.Id)
                .FirstOrDefaultAsync();

            return product == null ? null : _mapper.Map<ProductDTO>(product);
        }

        public async Task<GeneralResponse> RemoveBulkIdsAsync(string ids, Partner partner)
        {
            if (partner == null)
                return new GeneralResponse(false, "Invalid partner");

            var idList = ids.Split(',')
                .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            if (!idList.Any())
                return new GeneralResponse(false, "No valid IDs provided");

            var products = await appDbContext
                .Products.Where(p => idList.Contains(p.Id) && p.Partner.Id == partner.Id)
                .ToListAsync();

            if (!products.Any())
                return new GeneralResponse(false, "No matching products found");

            appDbContext.Products.RemoveRange(products);
            await appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, $"{products.Count} product(s) marked as deleted");
        }

        public async Task<List<OrderDetailDTO>> GetOrdersByProductIdAsync(
            int productId,
            Partner partner
        )
        {
            var orderIds = await appDbContext
                .Orders.Where(o => o.Partner.Id == partner.Id)
                .Select(o => o.Id)
                .ToListAsync();

            if (!orderIds.Any())
                return new List<OrderDetailDTO>();

            var orderDetails = await _ordersDetailsCollection
                .Find(od => orderIds.Contains(od.OrderId ?? 0) && od.ProductId == productId)
                .ToListAsync();

            // Step 3: Convert to DTOs
            return orderDetails
                .Select(od => new OrderDetailDTO
                {
                    Id = od.Id,
                    OrderId = od.OrderId ?? 0,
                    ProductId = od.ProductId,
                    ProductName = od.ProductName,
                    SaleOrderNo = od.SaleOrderNo,
                    PartnerName = od.PartnerName,
                    CustomerId = od.CustomerId,
                    CustomerName = od.CustomerName,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    Total = od.Total,
                    AmountSummary = od.AmountSummary,
                    DiscountAmount = od.DiscountAmount,
                    DiscountRate = od.DiscountRate,
                    TaxAmount = od.TaxAmount,
                    TaxID = od.TaxID,
                    TaxIDText = od.TaxIDText,
                    UsageUnitID = od.UsageUnitID,
                    CreatedAt = od.CreatedAt,
                    UpdatedAt = od.UpdatedAt,
                })
                .ToList();
        }

        public async Task<List<InvoiceDetailDTO>> GetInvoicesByProductIdAsync(
            int productId,
            Partner partner
        )
        {
            var invoiceIds = await appDbContext
                .Invoices.Where(o => o.Partner.Id == partner.Id)
                .Select(o => o.Id)
                .ToListAsync();

            if (!invoiceIds.Any())
                return new List<InvoiceDetailDTO>();

            var invoiceDetails = await _invoicesDetailsCollection
                .Find(od => invoiceIds.Contains(od.InvoiceId ?? 0) && od.ProductId == productId)
                .ToListAsync();

            return invoiceDetails
                .Select(od => new InvoiceDetailDTO
                {
                    Id = od.Id,
                    InvoiceId = od.InvoiceId ?? 0,
                    InvoiceRequestName = od.InvoiceRequestName,
                    ProductId = od.ProductId,
                    ProductName = od.ProductName,
                    SaleOrderNo = od.SaleOrderNo,
                    PartnerName = od.PartnerName,
                    CustomerId = od.CustomerId,
                    CustomerName = od.CustomerName,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    Total = od.Total,
                    AmountSummary = od.AmountSummary,
                    DiscountAmount = od.DiscountAmount,
                    DiscountRate = od.DiscountRate,
                    TaxAmount = od.TaxAmount,
                    TaxID = od.TaxID,
                    TaxIDText = od.TaxIDText,
                    UsageUnitID = od.UsageUnitID,
                    CreatedAt = od.CreatedAt,
                    UpdatedAt = od.UpdatedAt,
                })
                .ToList();
        }


        private async Task<ProductDTO> GetProductByCode(string code, Partner partner)
        {
            var existingProduct = await appDbContext.Products
                .FirstOrDefaultAsync(c => c.ProductCode == code && c.PartnerId == partner.Id);
            if (existingProduct == null)
                return null;

            return new ProductDTO
            {
                Id = existingProduct.Id,
                ProductCode = existingProduct.ProductCode,
                ProductName = existingProduct.ProductName,
            };
        }
        public async Task<DataObjectResponse?> GenerateProductCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(appDbContext);

            var productCode = await codeGenerator
            .GenerateNextCodeAsync<Product>(prefix: "HH",
                codeSelector: c => c.ProductCode,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã sản phẩm thành công", productCode);
        }

        public async Task<DataObjectResponse?> CheckProductCodeAsync(string code, Employee employee, Partner partner)
        {
            var productDetail = await GetProductByCode(code, partner);

            if (productDetail == null)
            {
                return new DataObjectResponse(true, "Mã sản phẩm có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã sản phẩm đã tồn tại", new
                {
                    productDetail.ProductCode,
                    productDetail.ProductName,
                    productDetail.Id
                });
            }
        }
    }
}
