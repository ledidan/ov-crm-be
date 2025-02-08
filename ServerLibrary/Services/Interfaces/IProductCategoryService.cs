using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductCategoryService
    {
        Task<GeneralResponse> CreateAsync(CreateProductCategory productCategory, Employee employee, Partner partner);
        Task<GeneralResponse> UpdateAsync(int id, UpdateProductCategoryDTO productCategory, Partner partner);
        Task<List<AllProductCategoryDTO>> GetAllAsync(Employee employee, Partner partner);
        Task<ProductCategoryDTO?> FindById(int id, Employee employee, Partner partner);

        Task<GeneralResponse> RemoveBulkIdsAsync(string ids, Partner partner);
       
    }
}
