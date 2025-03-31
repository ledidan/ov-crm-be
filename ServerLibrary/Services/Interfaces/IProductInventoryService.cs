using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IProductInventoryService
    {
        Task<DataObjectResponse> GetAllInventoriesAsync(Partner partner);
        Task<DataObjectResponse> GetInventoryByIdAsync(int id, Partner partner);
        Task<DataObjectResponse> CreateInventoryAsync(CreateInventoryDTO inventory, Partner partner);
        Task<DataObjectResponse> UpdateInventoryAsync(int id, UpdateInventoryDTO inventory, Partner partner);
        Task<GeneralResponse> DeleteInventoryAsync(int id, Partner partner);
        Task<DataObjectResponse> GetInventoriesByProductIdAsync(int productId, Partner partner);
    }
}