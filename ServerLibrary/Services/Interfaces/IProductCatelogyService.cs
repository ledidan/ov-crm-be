using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductCatelogyService
    {
        Task<GeneralResponse> CreateAsync(CreateProductCatelogy productCatelogy);
        Task<GeneralResponse> UpdateAsync(ProductCatelogy productCatelogy);
        Task<List<ProductCatelogy>> GetAllAsync(int partnerId);
    }
}
