using System.Security.Claims;
using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

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

        public async Task<DataObjectResponse> CreateInventoryAsync(
            CreateInventoryDTO inventoryDto,
            Partner partner
        )
        {
            try
            {
                if (inventoryDto == null)
                    return new DataObjectResponse(false, "Dữ liệu tồn kho không hợp lệ", null);

                if (partner == null || partner.Id <= 0)
                    return new DataObjectResponse(false, "Đối tác không hợp lệ", null);

                var existingPartner = await _context.Partners.FirstOrDefaultAsync(p =>
                    p.Id == partner.Id
                );
                if (existingPartner == null)
                    return new DataObjectResponse(
                        false,
                        $"Đối tác với ID {partner.Id} không tồn tại",
                        null
                    );

                var product = await _context.Products.FirstOrDefaultAsync(p =>
                    p.Id == inventoryDto.ProductId && p.Partner.Id == partner.Id
                );
                if (product == null)
                    return new DataObjectResponse(false, "Sản phẩm không thuộc đối tác này", null);

                if (inventoryDto.SupplierId.HasValue)
                {
                    var supplier = await _context.Suppliers.FirstOrDefaultAsync(s =>
                        s.Id == inventoryDto.SupplierId.Value
                    );
                    if (supplier == null)
                        return new DataObjectResponse(
                            false,
                            $"Nhà cung cấp với ID {inventoryDto.SupplierId.Value} không tồn tại",
                            null
                        );
                }

                var inventory = _mapper.Map<ProductInventory>(inventoryDto);

                // Gán PartnerId từ partner
                inventory.Partner = partner;

                inventory.SupplierId = inventoryDto.SupplierId;

                Console.WriteLine(
                    $"Before Save - ProductId: {inventory.ProductId}, PartnerId: {inventory.Partner.Id}, SupplierId: {inventory.SupplierId}"
                );

                _context.ProductInventories.Add(inventory);
                await _context.SaveChangesAsync();

                var responseDto = await MapToResponseDtoWithDb(inventory);
                return new DataObjectResponse(true, "Tạo tồn kho thành công", responseDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create inventory failed: {ex.Message}");
                return new DataObjectResponse(false, $"Tạo tồn kho thất bại: {ex.Message}", null);
            }
        }

        public async Task<GeneralResponse> DeleteInventoryAsync(int id, Partner partner)
        {
            var inventory = await _context
                .ProductInventories.Where(i => i.Id == id && i.Partner.Id == partner.Id)
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return new GeneralResponse(
                    false,
                    $"Không tìm thấy tồn kho với ID {id} cho đối tác này"
                );
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

        public async Task<List<InventoryDTO>> GetAllInventoriesAsync(Partner partner)
        {
            var inventories = await _context
                .ProductInventories.Where(i => i.Partner.Id == partner.Id)
                .Include(i => i.Product)
                .Include(i => i.Supplier)
                .Include(i => i.Partner)
                .ToListAsync();
            return inventories.Select(inventory => _mapper.Map<InventoryDTO>(inventory)).ToList();
        }

        public async Task<List<InventoryDTO?>> GetInventoriesByProductIdAsync(
            int productId,
            Partner partner
        )
        {
            var inventory = await _context
                .ProductInventories.Where(i => i.ProductId == productId && i.Partner.Id == partner.Id)
                .Include(i => i.Product)
                .Include(i => i.Supplier)
                .Include(i => i.Partner)
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return new List<InventoryDTO?>();
            }
            return new List<InventoryDTO?> { _mapper.Map<InventoryDTO>(inventory) };
        }

        public async Task<InventoryDTO> GetInventoryByIdAsync(int id, Partner partner)
        {
            var inventory = await _context
                .ProductInventories.Where(i => i.Id == id && i.Partner.Id == partner.Id)
                .Include(i => i.Product)
                .Include(i => i.Supplier)
                .Include(i => i.Partner)
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                return null;
            }
            return _mapper.Map<InventoryDTO>(inventory);
        }

        public async Task<DataObjectResponse> UpdateInventoryAsync(
            int id,
            UpdateInventoryDTO inventoryDto,
            Partner partner
        )
        {
            // Kiểm tra đầu vào
            if (inventoryDto == null)
                return new DataObjectResponse(false, "Dữ liệu cập nhật tồn kho không hợp lệ", null);

            if (partner == null || partner.Id <= 0)
                return new DataObjectResponse(false, "Đối tác không hợp lệ", null);

            // Tìm kiếm bản ghi tồn kho hiện tại
            var existingInventory = await _context
                .ProductInventories.Include(i => i.Product)
                .ThenInclude(p => p.Partner)
                .FirstOrDefaultAsync(i => i.Id == id && i.Product.Partner.Id == partner.Id);

            if (existingInventory == null)
                return new DataObjectResponse(
                    false,
                    $"Không tìm thấy tồn kho với ID {id} thuộc đối tác này",
                    null
                );

            try
            {
                if (
                    inventoryDto.ProductId != 0
                    && inventoryDto.ProductId != existingInventory.ProductId
                )
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p =>
                        p.Id == inventoryDto.ProductId != null && p.Partner.Id == partner.Id
                    );
                    if (product == null)
                        return new DataObjectResponse(
                            false,
                            $"Sản phẩm với ID {inventoryDto.ProductId} không thuộc đối tác này",
                            null
                        );
                }

                // Kiểm tra SupplierId nếu thay đổi
                if (inventoryDto.SupplierId.HasValue)
                {
                    var supplier = await _context.Suppliers.FirstOrDefaultAsync(s =>
                        s.Id == inventoryDto.SupplierId.Value
                    );
                    if (supplier == null)
                        return new DataObjectResponse(
                            false,
                            $"Nhà cung cấp với ID {inventoryDto.SupplierId.Value} không tồn tại",
                            null
                        );
                }

                // Log giá trị trước khi cập nhật
                Console.WriteLine(
                    $"Before Update - Id: {existingInventory.Id}, ProductId: {existingInventory.ProductId}, PartnerId: {existingInventory.Partner.Id}, SupplierId: {existingInventory.SupplierId}"
                );

                // Ánh xạ từ DTO sang entity
                _mapper.Map(inventoryDto, existingInventory);

                // Log giá trị sau khi ánh xạ
                Console.WriteLine(
                    $"After Mapping - Id: {existingInventory.Id}, ProductId: {existingInventory.ProductId}, PartnerId: {existingInventory.Partner.Id}, SupplierId: {existingInventory.SupplierId}"
                );

                // Lưu thay đổi
                await _context.SaveChangesAsync();

                // Ánh xạ sang Response DTO
                var responseDto = await MapToResponseDtoWithDb(existingInventory);
                return new DataObjectResponse(true, "Cập nhật tồn kho thành công", responseDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update inventory failed: {ex.Message}");
                return new DataObjectResponse(
                    false,
                    $"Cập nhật tồn kho thất bại: {ex.Message}",
                    null
                );
            }
        }

        private async Task<InventoryDTO> MapToResponseDtoWithDb(ProductInventory inventory)
        {
            var supplier = await _context.Suppliers.FindAsync(inventory.SupplierId);
            var product = await _context
                .Products.Include(p => p.Partner)
                .FirstOrDefaultAsync(p => p.Id == inventory.ProductId);

            var responseDto = _mapper.Map<InventoryDTO>(inventory);
            responseDto.SupplierName = supplier?.SupplierName;
            responseDto.PartnerId = product?.Partner?.Id ?? 0;
            return responseDto;
        }
    }
}
