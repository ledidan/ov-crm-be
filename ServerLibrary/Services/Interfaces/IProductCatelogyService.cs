using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductCatelogyService
    {
        Task<GeneralResponse> CreateAsync(CreateProductCatelogy productCatelogy, Partner partner);
        Task<GeneralResponse> UpdateAsync(ProductCatelogy productCatelogy);
        Task<List<ProductCatelogy>> GetAllAsync(Partner partner);
        Task<ProductCatelogy?> FindById(int id);
    }
}
