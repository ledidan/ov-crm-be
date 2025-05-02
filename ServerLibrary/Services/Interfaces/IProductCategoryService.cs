using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductCategoryService
    {
        Task<DataObjectResponse?> GenerateProductCategoryCodeAsync(Partner partner);
        Task<DataObjectResponse?> CheckProductCategoryCodeAsync(string code, Employee employee, Partner partner);

        Task<GeneralResponse> CreateAsync(CreateProductCategory productCategory, Employee employee, Partner partner);
        Task<GeneralResponse> UpdateAsync(int id, UpdateProductCategoryDTO productCategory, Partner partner, Employee employee);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateProductCategoryDTO productCategory, Employee employee, Partner partner);
        Task<PagedResponse<List<AllProductCategoryDTO>>> GetAllAsync(Employee employee, Partner partner, int pageNumber, int pageSize);
        Task<ProductCategoryDTO?> FindById(int id, Employee employee, Partner partner);
        Task<GeneralResponse> RemoveBulkIdsAsync(string ids, Partner partner);
    }
}
