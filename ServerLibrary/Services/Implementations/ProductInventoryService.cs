using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;

namespace ServerLibrary.Services.Implementations
{
    public class ProductInventoryService : IProductInventoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public ProductInventoryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<DataObjectResponse> CreateInventoryAsync(CreateInventoryDTO inventoryDto, Partner partner)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == inventoryDto.ProductId && p.Partner.Id == partner.Id);
                if (product == null)
                {
                    return new DataObjectResponse(false, "Sản phẩm không thuộc đối tác này", null);
                }

                // Ánh xạ từ CreateInventoryDTO sang ProductInventory
                var inventory = _mapper.Map<ProductInventory>(inventoryDto);
                _context.ProductInventories.Add(inventory);
                await _context.SaveChangesAsync();

                // Ánh xạ sang InventoryDTO với dữ liệu bổ sung
                var responseDto = await MapToResponseDtoWithDb(inventory);
                return new DataObjectResponse(true, "Tạo tồn kho thành công", responseDto);
            }
            catch (Exception ex)
            {
                return new DataObjectResponse(false, $"Tạo tồn kho thất bại: {ex.Message}", null);
            }
        }

        public async Task<GeneralResponse> DeleteInventoryAsync(int id, Partner partner)
        {
            var inventory = await _context.ProductInventories
                 .Where(i => i.Id == id && i.Partner.Id == partner.Id)
                 .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return new GeneralResponse(false, $"Không tìm thấy tồn kho với ID {id} cho đối tác này");
            }

            try
            {
                _context.ProductInventories.Remove(inventory);
                await _context.SaveChangesAsync();
                return new GeneralResponse(true, "Xóa tồn kho thành công");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"Xóa tồn kho thất bại: {ex.Message}");
            }
        }

        public async Task<DataObjectResponse> GetAllInventoriesAsync(Partner partner)
        {
            var inventories = await _context.ProductInventories
                            .Where(i => i.Partner.Id == partner.Id)
                            .Include(i => i.Product)
                            .Include(i => i.Supplier)
                            .Include(i => i.Partner)
                            .ToListAsync();
            return new DataObjectResponse(true, "Lấy danh sách tồn kho thành công", inventories);
        }

        public async Task<DataObjectResponse> GetInventoriesByProductIdAsync(int productId, Partner partner)
        {
            var inventory = await _context.ProductInventories
                .Where(i => i.Id == productId && i.Partner.Id == partner.Id)
                .Include(i => i.Product)
                .Include(i => i.Supplier)
                .Include(i => i.Partner)
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return new DataObjectResponse(false, $"Không tìm thấy tồn kho với ID {productId} cho đối tác này", null);
            }
            return new DataObjectResponse(true, "Lấy thông tin tồn kho thành công", inventory);
        }

        public async Task<DataObjectResponse> GetInventoryByIdAsync(int id, Partner partner)
        {
            var inventory = await _context.ProductInventories
                  .Where(i => i.Id == id && i.Partner.Id == partner.Id)
                  .Include(i => i.Product)
                  .Include(i => i.Supplier)
                  .Include(i => i.Partner)
                  .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return new DataObjectResponse(false, $"Không tìm thấy tồn kho với ID {id} cho đối tác này", null);
            }
            return new DataObjectResponse(true, "Lấy thông tin tồn kho thành công", inventory);
        }

        public async Task<DataObjectResponse> UpdateInventoryAsync(int id, UpdateInventoryDTO inventoryDto, Partner partner)
        {
            var existingInventory = await _context.ProductInventories
                .Include(i => i.Product)
                .ThenInclude(p => p.Partner)
                .FirstOrDefaultAsync(i => i.Id == id && i.Product.Partner.Id == partner.Id);

            if (existingInventory == null)
            {
                return new DataObjectResponse(false, $"Không tìm thấy tồn kho với ID {id} thuộc đối tác này", null);
            }

            try
            {
                if (inventoryDto.ProductId != existingInventory.ProductId)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == inventoryDto.ProductId && p.Partner.Id == partner.Id);
                    if (product == null)
                    {
                        return new DataObjectResponse(false, "Sản phẩm mới không thuộc đối tác này", null);
                    }
                }

                // Sử dụng IMapper để ánh xạ từ DTO sang Entity
                _mapper.Map(inventoryDto, existingInventory); // Cập nhật trực tiếp lên existingInventory
                await _context.SaveChangesAsync();

                // Ánh xạ sang Response DTO với dữ liệu bổ sung
                var responseDto = await MapToResponseDtoWithDb(existingInventory);
                return new DataObjectResponse(true, "Cập nhật tồn kho thành công", responseDto);
            }
            catch (Exception ex)
            {
                return new DataObjectResponse(false, $"Cập nhật tồn kho thất bại: {ex.Message}", null);
            }

        }
        private async Task<InventoryDTO> MapToResponseDtoWithDb(ProductInventory inventory)
        {
            var supplier = await _context.Suppliers.FindAsync(inventory.SupplierId);
            var product = await _context.Products
                .Include(p => p.Partner)
                .FirstOrDefaultAsync(p => p.Id == inventory.ProductId);

            var responseDto = _mapper.Map<InventoryDTO>(inventory);
            responseDto.SupplierName = supplier?.SupplierName;
            responseDto.PartnerId = product?.Partner?.Id ?? 0;
            return responseDto;
        }
    }
}
