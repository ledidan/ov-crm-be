using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly AppDbContext appDbContext;
        private readonly S3Service s3Service;
        private readonly IMapper mapper;

        public ProductCategoryService(
            AppDbContext appDbContext,
            S3Service s3Service,
            IMapper mapper
        )
        {
            this.appDbContext = appDbContext;
            this.s3Service = s3Service;
            this.mapper = mapper;
        }

        public async Task<GeneralResponse> CreateAsync(
            CreateProductCategory productCategory,
            Employee employee,
            Partner partner
        )
        {
            string? avatarUrl = null;
            if (
                !string.IsNullOrEmpty(productCategory.Avatar)
                && productCategory.Avatar.StartsWith("data:image")
            )
            {
                avatarUrl = await s3Service.UploadBase64ImageToS3(
                    productCategory.Avatar,
                    "product-category",
                    partner.Id,
                    1,
                    "crm",
                    "partner"
                );
            }
            await appDbContext.InsertIntoDb(
                new ProductCategory()
                {
                    Avatar = avatarUrl,
                    ProductCategoryCode = productCategory.ProductCategoryCode,
                    ProductCategoryName = productCategory.ProductCategoryName,
                    ParentProductCategoryID = productCategory.ParentProductCategoryID,
                    InventoryCategoryID = productCategory.InventoryCategoryID,
                    InActive = productCategory.InActive,
                    ModifiedBy = productCategory.ModifiedBy,
                    Description = productCategory.Description,
                    OwnerId = employee.Id,
                    Partner = partner,
                }
            );

            return new GeneralResponse(true, "Tạo loại hàng hoá thành công");
        }

        public async Task<List<AllProductCategoryDTO>> GetAllAsync(
            Employee employee,
            Partner partner
        )
        {
            var categories = await appDbContext
                .ProductCategories.Where(pc =>
                    pc.Partner.Id == partner.Id && pc.OwnerId == employee.Id
                )
                .ToListAsync();

            List<AllProductCategoryDTO> flattenedCategories = new List<AllProductCategoryDTO>();

            foreach (var category in categories)
            {
                var categoryDTO = new AllProductCategoryDTO
                {
                    Id = category.Id,
                    ProductCategoryCode = category.ProductCategoryCode,
                    ProductCategoryName = category.ProductCategoryName,
                    ParentProductCategoryID = category.ParentProductCategoryID,
                };

                flattenedCategories.Add(categoryDTO);
            }

            return flattenedCategories;
        }

        public async Task<GeneralResponse> UpdateAsync(
            int id,
            UpdateProductCategoryDTO productCategory,
            Partner partner,
            Employee employee
        )
        {
            if (productCategory == null || id <= 0)
            {
                return new GeneralResponse(false, "Invalid product category data provided.");
            }

            var existingCategory = await appDbContext.ProductCategories.FirstOrDefaultAsync(pc =>
                pc.Id == id && pc.Partner.Id == partner.Id
            );

            if (existingCategory == null)
            {
                return new GeneralResponse(
                    false,
                    "Không tìm thấy danh mục sản phẩm hoặc danh mục này không thuộc về tổ chức đã chỉ định."
                );
            }

            // !!Optional: Prevent duplicate category names within the same partner
            // var isDuplicate = await appDbContext.ProductCategories
            //     .AnyAsync(pc => pc.Id != id && pc.Partner.Id == partner.Id && pc.ProductCategoryName == productCategory.ProductCategoryName);

            if (productCategory.ParentProductCategoryID.HasValue)
            {
                if (productCategory.ParentProductCategoryID == id)
                {
                    return new GeneralResponse(
                        false,
                        "Một danh mục không thể là danh mục cha của chính nó."
                    );
                }

                var parentCategory = await appDbContext.ProductCategories.FirstOrDefaultAsync(pc =>
                    pc.Id == productCategory.ParentProductCategoryID.Value
                );

                if (parentCategory == null || parentCategory.InActive == false)
                {
                    return new GeneralResponse(
                        false,
                        "Danh mục cha được chỉ định không tồn tại hoặc không hoạt động."
                    );
                }
            }
            string? avatarUrl = null;
            if (
                !string.IsNullOrEmpty(productCategory.Avatar)
                && productCategory.Avatar.StartsWith("data:image")
            )
            {
                avatarUrl = await s3Service.UploadBase64ImageToS3(
                    productCategory.Avatar,
                    "product-category",
                    partner.Id,
                    employee.Id,
                    "crm",
                    "partner"
                );
            }
            else if (
                (
                    productCategory.Avatar == null
                    || string.IsNullOrEmpty(productCategory.Avatar)
                    || productCategory.Avatar == "null"
                ) && !string.IsNullOrEmpty(existingCategory.Avatar)
            )
            {
                Console.WriteLine("Condition triggered: Removing existing avatar from S3");
                await s3Service.RemoveFileFromS3(existingCategory.Avatar);
                avatarUrl = null;
            }
            existingCategory.Avatar = avatarUrl ?? existingCategory.Avatar;
            existingCategory.ProductCategoryCode = productCategory.ProductCategoryCode;
            existingCategory.ProductCategoryName = productCategory.ProductCategoryName;
            existingCategory.ParentProductCategoryID = productCategory.ParentProductCategoryID;
            existingCategory.Description = productCategory.Description;
            existingCategory.InActive = productCategory.InActive;
            existingCategory.ModifiedBy = productCategory.ModifiedBy;

            await appDbContext.UpdateDb(existingCategory);

            return new GeneralResponse(true, "Đã cập nhật danh mục sản phẩm thành công.");
        }

        public async Task<ProductCategoryDTO?> FindById(int id, Employee employee, Partner partner)
        {
            if (employee == null || partner == null)
                return null;

            var categories = await appDbContext
                .ProductCategories.Where(pc =>
                    pc.Partner.Id == partner.Id && pc.OwnerId == employee.Id
                )
                .ToListAsync();

            var categoryDict = categories.ToDictionary(
                c => c.Id,
                c => new ProductCategoryDTO
                {
                    Id = c.Id,
                    Avatar = c.Avatar,
                    ProductCategoryCode = c.ProductCategoryCode,
                    ProductCategoryName = c.ProductCategoryName,
                    Description = c.Description,
                    IsPublic = c.IsPublic,
                    InActive = c.InActive,
                    ModifiedBy = c.ModifiedBy,
                    OwnerId = c.OwnerId,
                    SubCategories = new List<ProductCategoryDTO>(),
                }
            );

            foreach (var cat in categories)
            {
                if (
                    cat.ParentProductCategoryID.HasValue
                    && categoryDict.ContainsKey(cat.ParentProductCategoryID.Value)
                )
                {
                    categoryDict[cat.ParentProductCategoryID.Value]
                        .SubCategories.Add(categoryDict[cat.Id]);
                }
            }
            return categoryDict.TryGetValue(id, out var category) ? category : null;
        }

        public Task<GeneralResponse> UpdateAsync(ProductCategoryDTO productCategory)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse> RemoveBulkIdsAsync(string ids, Partner partner)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return new GeneralResponse(false, "No category IDs provided for deletion.");
            }

            // Convert comma-separated string into a List<int>
            var idList = ids.Split(',')
                .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            if (!idList.Any())
            {
                return new GeneralResponse(false, "Invalid category IDs provided.");
            }

            var categories = await appDbContext
                .ProductCategories.Where(pc =>
                    idList.Contains(pc.Id) && pc.Partner.Id == partner.Id
                )
                .Include(pc => pc.SubCategories)
                .ToListAsync();

            if (!categories.Any())
            {
                return new GeneralResponse(false, "Categories not found.");
            }

            List<int> removedIds = new List<int>();
            List<int> failedIds = new List<int>();

            foreach (var category in categories)
            {
                if (category.SubCategories.Any())
                {
                    failedIds.Add(category.Id);
                }
                else
                {
                    appDbContext.ProductCategories.Remove(category);
                    removedIds.Add(category.Id);
                }
            }

            await appDbContext.SaveChangesAsync();

            string removedString = removedIds.Any()
                ? $"Removed: {string.Join(", ", removedIds)}"
                : "No categories removed.";
            string failedString = failedIds.Any()
                ? $"Cannot remove (has subcategories): {string.Join(", ", failedIds)}"
                : "";

            return new GeneralResponse(true, $"{removedString} {failedString}".Trim());
        }

        public async Task<GeneralResponse?> UpdateFieldIdAsync(
            int id,
            UpdateProductCategoryDTO? productCategory,
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
                    // Kiểm tra tham số đầu vào
                    if (productCategory == null || id <= 0)
                        return new GeneralResponse(
                            false,
                            "Dữ liệu danh mục sản phẩm không hợp lệ hoặc ID không hợp lệ"
                        );

                    if (partner == null)
                        return new GeneralResponse(false, "Đối tác không hợp lệ");

                    if (employee == null)
                        return new GeneralResponse(false, "Nhân viên không hợp lệ");

                    // Tìm danh mục hiện tại
                    var existingCategory = await appDbContext.ProductCategories.FirstOrDefaultAsync(
                        pc => pc.Id == id && pc.Partner.Id == partner.Id
                    );

                    if (existingCategory == null)
                        return new GeneralResponse(
                            false,
                            $"Không tìm thấy danh mục sản phẩm với ID {id} thuộc đối tác {partner.Id}"
                        );

                    // Validate ParentProductCategoryID trước khi cập nhật
                    if (productCategory.ParentProductCategoryID.HasValue)
                    {
                        Console.WriteLine(
                            $"ParentProductCategoryID: {productCategory.ParentProductCategoryID}"
                        );
                        if (productCategory.ParentProductCategoryID == id)
                            return new GeneralResponse(
                                false,
                                "Danh mục không thể là cha của chính nó"
                            );

                        var parentCategory =
                            await appDbContext.ProductCategories.FirstOrDefaultAsync(pc =>
                                pc.Id == productCategory.ParentProductCategoryID.Value
                            );

                        // if (parentCategory == null)
                        //     return new GeneralResponse(
                        //         false,
                        //         "Danh mục cha được chỉ định không tồn tại"
                        //     );

                        // Cập nhật ParentProductCategoryID
                        existingCategory.ParentProductCategoryID = productCategory
                            .ParentProductCategoryID
                            .Value;
                        appDbContext
                            .Entry(existingCategory)
                            .Property("ParentProductCategoryID")
                            .IsModified = true;
                    }
                    else if (
                        !productCategory.ParentProductCategoryID.HasValue
                        && existingCategory.ParentProductCategoryID != null
                    )
                    {
                        // Xóa ParentProductCategoryID nếu DTO gửi null
                        existingCategory.ParentProductCategoryID = null;
                        appDbContext
                            .Entry(existingCategory)
                            .Property("ParentProductCategoryID")
                            .IsModified = true;
                    }

                    var properties = typeof(UpdateProductCategoryDTO)
                        .GetProperties()
                        .Where(p => p.Name != "ParentProductCategoryID");
                    foreach (var prop in properties)
                    {
                        var newValue = prop.GetValue(productCategory);
                        if (newValue != null && newValue.ToString() != "")
                        {
                            var existingProp = typeof(ProductCategory).GetProperty(prop.Name);
                            if (existingProp != null && existingProp.CanWrite && prop.Name != "Id")
                            {
                                existingProp.SetValue(existingCategory, newValue);
                                appDbContext
                                    .Entry(existingCategory)
                                    .Property(existingProp.Name)
                                    .IsModified = true;
                            }
                        }
                    }

                    // Log trước khi lưu
                    Console.WriteLine(
                        $"Before Save - Id: {existingCategory.Id}, ParentProductCategoryID: {existingCategory.ParentProductCategoryID}"
                    );

                    // Lưu thay đổi
                    await appDbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new GeneralResponse(true, "Cập nhật danh mục sản phẩm thành công");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Update failed: {ex.Message}");
                    return new GeneralResponse(
                        false,
                        $"Cập nhật danh mục sản phẩm thất bại: {ex.Message}"
                    );
                }
            });
        }
    }
}
