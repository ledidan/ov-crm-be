using Data.DTOs;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductInventoryService
    {
        Task<DataObjectResponse?> GenerateInventoryCodeAsync(Partner partner);
        Task<DataObjectResponse?> CheckInventoryCodeAsync(string code, Employee employee, Partner partner);
        Task<PagedResponse<List<InventoryDTO>>> GetAllInventoriesAsync(Partner partner, int pageNumber, int pageSize);
        Task<InventoryDTO> GetInventoryByIdAsync(int id, Partner partner);
        Task<DataObjectResponse> CreateInventoryAsync(CreateInventoryDTO inventory, Partner partner);
        Task<DataObjectResponse> UpdateInventoryAsync(int id, UpdateInventoryDTO inventory, Partner partner);
        Task<GeneralResponse> DeleteInventoryAsync(int id, Partner partner);
        Task<List<InventoryDTO?>> GetInventoriesByProductIdAsync(int productId, Partner partner);

        Task<GeneralResponse> CheckAndUpdateStockAsync(int productId, int quantity);
        Task<GeneralResponse> ReceiveStockAsync(int productId, int quantity, Partner partner);
    }
}