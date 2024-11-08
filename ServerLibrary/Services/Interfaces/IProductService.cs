using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductService
    {
        Task<GeneralResponse> CreateAsync(CreateProduct product, Partner partner);
        Task<GeneralResponse> UpdateAsync(Product product);
        Task<List<Product>> GetAllAsync(Partner partner);
        Task<GeneralResponse> UpdateSellingPriceAsync(Product product, double sellingPrice);
    }
}
