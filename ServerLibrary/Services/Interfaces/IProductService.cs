using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductService
    {
        Task<GeneralResponse> CreateAsync(CreateProduct product, Employee employee, Partner partner);
        Task<List<ProductDTO>> GetAllAsync(Employee employee, Partner partner);
        // Task<GeneralResponse> UpdateAsync(Product product);
        // Task<GeneralResponse> UpdateSellingPriceAsync(Product product, double sellingPrice);
    }
}
