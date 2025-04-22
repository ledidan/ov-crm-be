using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductService
    {
        Task<DataObjectResponse?> GenerateProductCodeAsync(Partner partner);
        Task<DataObjectResponse?> CheckProductCodeAsync(string code, Employee employee, Partner partner);

        Task<GeneralResponse> CreateAsync(CreateProductDTO product, Employee employee, Partner partner);

        Task<List<ProductDTO>> GetAllAsync(Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateProductDTO product, Employee employee, Partner partner);

        Task<ProductDTO?> FindByIdAsync(int id, Partner partner);
        Task<GeneralResponse> UpdateAsync(int id, UpdateProductDTO product, Partner partner, Employee employee);


        Task<List<OrderDetailDTO>> GetOrdersByProductIdAsync(int productId, Partner partner);

        Task<List<InvoiceDetailDTO>> GetInvoicesByProductIdAsync(int productId, Partner partner);
        Task<GeneralResponse> RemoveBulkIdsAsync(string ids, Partner partner);
        // Task<GeneralResponse> UpdateSellingPriceAsync(Product product, double sellingPrice);
    }
}
