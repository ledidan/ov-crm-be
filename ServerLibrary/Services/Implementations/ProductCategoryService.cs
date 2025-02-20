using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ProductCategoryService(AppDbContext appDbContext, S3Service s3Service, IMapper mapper) : IProductCategoryService
    {
        public async Task<GeneralResponse> CreateAsync(CreateProductCategory productCategory, Employee employee, Partner partner)
        {
            string? avatarUrl = null;
            if (!string.IsNullOrEmpty(productCategory.Avatar) && productCategory.Avatar.StartsWith("data:image"))
            {
                avatarUrl = await s3Service.UploadBase64ImageToS3(
                    productCategory.Avatar,
                    "product-category",
                    partner.Id, 1,
                    "crm",
                    "partner"
                );
            }
            await appDbContext.InsertIntoDb(new ProductCategory()
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
                Partner = partner
            });

            return new GeneralResponse(true, "Product Category created");
        }
        public async Task<List<AllProductCategoryDTO>> GetAllAsync(Employee employee, Partner partner)
        {
            var categories = await appDbContext.ProductCategories
                .Where(pc => pc.Partner.Id == partner.Id && pc.OwnerId == employee.Id)
                .ToListAsync();

            List<AllProductCategoryDTO> flattenedCategories = new List<AllProductCategoryDTO>();

            foreach (var category in categories)
            {
                var categoryDTO = new AllProductCategoryDTO
                {
                    Id = category.Id,
                    ProductCategoryCode = category.ProductCategoryCode,
                    ProductCategoryName = category.ProductCategoryName,
                    ParentProductCategoryID = category.ParentProductCategoryID
                };

                flattenedCategories.Add(categoryDTO);
            }

            return flattenedCategories;
        }

        // public async Task<List<ProductCategoryDTO>> GetAllAsync(Employee employee, Partner partner)
        // {
        //     var categories = await appDbContext.ProductCategories
        //         .Where(pc => pc.Partner.Id == partner.Id && pc.OwnerId == employee.Id)
        //         .ToListAsync();
        //     var categoryDict = categories.ToDictionary(c => c.Id, c => new ProductCategoryDTO
        //     {
        //         Id = c.Id,
        //         ProductCategoryCode = c.ProductCategoryCode,
        //         ProductCategoryName = c.ProductCategoryName,
        //         SubCategories = new List<ProductCategoryDTO>()
        //     });

        //     List<ProductCategoryDTO> rootCategories = new List<ProductCategoryDTO>();

        //     foreach (var category in categories)
        //     {
        //         if (category.ParentProductCategoryID == null)
        //         {
        //             rootCategories.Add(categoryDict[category.Id]);
        //         }
        //         else if (categoryDict.ContainsKey(category.ParentProductCategoryID.Value))
        //         {
        //             categoryDict[category.ParentProductCategoryID.Value].SubCategories.Add(categoryDict[category.Id]);
        //         }
        //     }

        //     return rootCategories;
        // }
        public async Task<GeneralResponse> UpdateAsync(int id, UpdateProductCategoryDTO productCategory, Partner partner)
        {
            if (productCategory == null || id <= 0)
            {
                return new GeneralResponse(false, "Invalid product category data provided.");
            }

            var existingCategory = await appDbContext.ProductCategories
                .FirstOrDefaultAsync(pc => pc.Id == id && pc.Partner.Id == partner.Id);

            if (existingCategory == null)
            {
                return new GeneralResponse(false, "Product category not found or does not belong to the specified partner.");
            }

            // !!Optional: Prevent duplicate category names within the same partner
            // var isDuplicate = await appDbContext.ProductCategories
            //     .AnyAsync(pc => pc.Id != id && pc.Partner.Id == partner.Id && pc.ProductCategoryName == productCategory.ProductCategoryName);

            // if (isDuplicate)
            // {
            //     return new GeneralResponse(false, "A category with this name already exists.");
            // }
            if (productCategory.ParentProductCategoryID.HasValue)
            {
                if (productCategory.ParentProductCategoryID == id)
                {
                    return new GeneralResponse(false, "A category cannot be its own parent.");
                }

                var parentCategory = await appDbContext.ProductCategories
                    .FirstOrDefaultAsync(pc => pc.Id == productCategory.ParentProductCategoryID.Value);

                if (parentCategory == null || parentCategory.InActive == false)
                {
                    return new GeneralResponse(false, "The specified parent category does not exist or is inactive.");
                }
            }
            existingCategory.Avatar = productCategory.Avatar;
            existingCategory.ProductCategoryCode = productCategory.ProductCategoryCode;
            existingCategory.ProductCategoryName = productCategory.ProductCategoryName;
            existingCategory.ParentProductCategoryID = productCategory.ParentProductCategoryID;
            existingCategory.Description = productCategory.Description;
            existingCategory.InActive = productCategory.InActive;
            existingCategory.ModifiedBy = productCategory.ModifiedBy;

            await appDbContext.UpdateDb(existingCategory);

            return new GeneralResponse(true, "Product category updated successfully.");
        }


        public async Task<ProductCategoryDTO?> FindById(int id, Employee employee, Partner partner)
        {
            if (employee == null || partner == null)
                return null;

            var categories = await appDbContext.ProductCategories
                .Where(pc => pc.Partner.Id == partner.Id && pc.OwnerId == employee.Id)
                .ToListAsync();

            var categoryDict = categories.ToDictionary(c => c.Id, c => new ProductCategoryDTO
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
                SubCategories = new List<ProductCategoryDTO>()
            });

            foreach (var cat in categories)
            {
                if (cat.ParentProductCategoryID.HasValue && categoryDict.ContainsKey(cat.ParentProductCategoryID.Value))
                {
                    categoryDict[cat.ParentProductCategoryID.Value].SubCategories.Add(categoryDict[cat.Id]);
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

            var categories = await appDbContext.ProductCategories
                .Where(pc => idList.Contains(pc.Id) && pc.Partner.Id == partner.Id)
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

            string removedString = removedIds.Any() ? $"Removed: {string.Join(", ", removedIds)}" : "No categories removed.";
            string failedString = failedIds.Any() ? $"Cannot remove (has subcategories): {string.Join(", ", failedIds)}" : "";

            return new GeneralResponse(true, $"{removedString} {failedString}".Trim());
        }

        public async Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateProductCategoryDTO productCategory, Employee employee, Partner partner)
        {
            if (productCategory == null || id <= 0)
            {
                return new GeneralResponse(false, "Invalid product category data provided.");
            }

            if (partner == null)
            {
                return new GeneralResponse(false, "Invalid partner.");
            }

            var existingCategory = await appDbContext.ProductCategories
                .FirstOrDefaultAsync(pc => pc.Id == id && pc.Partner.Id == partner.Id);

            if (existingCategory == null)
            {
                return new GeneralResponse(false, "Product category not found or does not belong to the specified partner.");
            }

            // Validate Parent Category
            if (productCategory.ParentProductCategoryID.HasValue)
            {
                if (productCategory.ParentProductCategoryID == id)
                {
                    return new GeneralResponse(false, "A category cannot be its own parent.");
                }

                // var parentCategory = await appDbContext.ProductCategories
                //     .FirstOrDefaultAsync(pc => pc.Id == productCategory.ParentProductCategoryID.Value);

                // if (parentCategory == null || parentCategory.InActive == false)
                // {
                //     return new GeneralResponse(false, "The specified parent category does not exist or is inactive.");
                // }
            }

            mapper.Map(productCategory, existingCategory);

            appDbContext.Entry(existingCategory).State = EntityState.Modified;

            await appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Product category updated successfully.");
        }
    }
}
