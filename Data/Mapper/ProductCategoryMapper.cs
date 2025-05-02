
using Data.DTOs;
using Data.Entities;
using Mapper.ContactMapper;

namespace Mapper.ProductCategoryMapper
{
    public static class ProductCategoryMapper
    {
        public static ProductCategoryDTO ToProductCategoryDTO(this ProductCategory productCategoryModel)
        {
            return new ProductCategoryDTO
            {
                ProductCategoryCode = productCategoryModel.ProductCategoryCode,
                ProductCategoryName = productCategoryModel.ProductCategoryName,
            };
        }
        public static ProductCategoryDTO ToUpdateCategoryDTO(this ProductCategory productCategoryModel)
        {
            return new ProductCategoryDTO
            {
                ProductCategoryCode = productCategoryModel.ProductCategoryCode,
                ProductCategoryName = productCategoryModel.ProductCategoryName,
                ParentProductCategoryID= productCategoryModel.ParentProductCategoryID,
                Description = productCategoryModel.Description,
                InActive = productCategoryModel.InActive,
            };
        }
    }
}