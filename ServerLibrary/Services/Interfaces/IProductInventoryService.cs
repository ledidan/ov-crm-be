using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductInventoryService
    {
        Task<List<InventoryDTO>> GetAllInventoriesAsync(Partner partner);
        Task<InventoryDTO> GetInventoryByIdAsync(int id, Partner partner);
        Task<DataObjectResponse> CreateInventoryAsync(CreateInventoryDTO inventory, Partner partner);
        Task<DataObjectResponse> UpdateInventoryAsync(int id, UpdateInventoryDTO inventory, Partner partner);
        Task<GeneralResponse> DeleteInventoryAsync(int id, Partner partner);
        Task<List<InventoryDTO?>> GetInventoriesByProductIdAsync(int productId, Partner partner);
    }
}