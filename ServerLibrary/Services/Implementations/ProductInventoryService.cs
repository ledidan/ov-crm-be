using System.Security.Claims;
using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ProductInventoryService : IProductInventoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPartnerService _partnerService;
        public ProductInventoryService(AppDbContext context, IMapper mapper, IPartnerService partnerService)
        {
            _context = context;
            _mapper = mapper;
            _partnerService = partnerService;
        }

        public async Task<GeneralResponse> CheckAndUpdateStockAsync(int productId, int quantity)
        {
            var inventory = await _context.ProductInventories.FirstOrDefaultAsync(
                i => i.ProductId == productId
            );
            var product = await _context.Products.FindAsync(productId);

            if (product == null) return new GeneralResponse(false, "Sản phẩm không tồn tại");

            if (inventory == null) return new GeneralResponse(false, $"Chưa có dữ liệu kho cho sản phẩm {product.ProductName}!");
            if (inventory.QuantityInStock < quantity)
                return new GeneralResponse(false, $"Hàng {product.ProductName} không đủ, chỉ còn {inventory.QuantityInStock} cái!");

            inventory.QuantityInStock -= quantity;
            return new GeneralResponse(true, null);
        }

        public async Task<DataObjectResponse> CreateInventoryAsync(
            CreateInventoryDTO inventoryDto,
            Partner partner
        )
        {
            var codeGenerator = new GenerateNextCode(_context);

            try
            {
                if (inventoryDto == null)
                    return new DataObjectResponse(false, "Dữ liệu tồn kho không hợp lệ", null);

                if (partner == null)
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

                if (string.IsNullOrEmpty(inventory.InventoryCode))
                {
                    inventory.InventoryCode = await codeGenerator.GenerateNextCodeAsync<ProductInventory>("TK", c => c.InventoryCode, c => c.PartnerId == partner.Id);
                }
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

        public async Task<PagedResponse<List<InventoryDTO>>> GetAllInventoriesAsync(Partner partner, int pageNumber, int pageSize)
        {
            if (partner == null)
            {
                throw new ArgumentNullException(nameof(partner), "Partner không tồn tại");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10; // Default page size

            var query = _context.ProductInventories
                .Where(i => i.Partner.Id == partner.Id)
                .Include(i => i.Product)
                .Include(i => i.Supplier)
                .Include(i => i.Partner);

            var totalRecords = await query.CountAsync();

            var inventories = await query
                .OrderBy(i => i.Id) // Add sorting for consistency
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var inventoryDTOs = new List<InventoryDTO>();
            foreach (var inventory in inventories)
            {
                var dto = await MapToResponseDtoWithDb(inventory);
                inventoryDTOs.Add(dto);
            }

            return new PagedResponse<List<InventoryDTO>>(
                data: inventoryDTOs,
                pageNumber: pageNumber,
                pageSize: pageSize,
                totalRecords: totalRecords
            );
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
            return await MapToResponseDtoWithDb(inventory);
        }

        public async Task<GeneralResponse> ReceiveStockAsync(int productId, int quantity, Partner partner)
        {
            if (quantity <= 0)
                return new GeneralResponse(false, "Số lượng nhập kho phải lớn hơn 0!");

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return new GeneralResponse(false, $"Sản phẩm {productId} không tồn tại.");

            var inventory = await _context.ProductInventories
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null)
            {
                inventory = new ProductInventory
                {
                    ProductId = productId,
                    QuantityInStock = quantity,
                    Partner = partner,
                };
                _context.ProductInventories.Add(inventory);
            }
            else
            {
                inventory.QuantityInStock += quantity;
            }
            await _context.SaveChangesAsync();

            return new GeneralResponse(true, $"Nhập {quantity} cái cho sản phẩm {product.ProductName} ok!");
        }

        public async Task<DataObjectResponse> UpdateInventoryAsync(
            int id,
            UpdateInventoryDTO inventoryDto,
            Partner partner
        )
        {
            var codeGenerator = new GenerateNextCode(_context);

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
            if (string.IsNullOrEmpty(inventoryDto.InventoryCode))
            {
                inventoryDto.InventoryCode = await codeGenerator.GenerateNextCodeAsync<ProductInventory>(
                    "TK",
                    c => c.InventoryCode,
                    c => c.PartnerId == partner.Id
                );
            }
            else
            {
                bool exists = await _context.ProductInventories.AnyAsync(c =>
                    c.InventoryCode == inventoryDto.InventoryCode &&
                    c.PartnerId == partner.Id &&
                    c.Id != id);

                if (exists)
                {
                    var newInventoryCode = await codeGenerator.GenerateNextCodeAsync<ProductInventory>(
                        "TK",
                        c => c.InventoryCode,
                        c => c.PartnerId == partner.Id
                    );
                    inventoryDto.InventoryCode = newInventoryCode;
                }
            }
            try
            {
                if (
                    inventoryDto.ProductId != 0
                    && inventoryDto.ProductId != existingInventory.ProductId
                )
                {
                    var product = await _context.Products
     .Include(p => p.Partner)
     .FirstOrDefaultAsync(p =>
         p.Id == inventoryDto.ProductId &&
         p.Partner.Id == partner.Id
     );
                    Console.WriteLine($"Updating ProductInventory with ProductId: {inventoryDto.ProductId}");

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
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == inventory.ProductId);

            // Truy vấn Partner để lấy PartnerName
            var partner = await _context.Partners
                .FirstOrDefaultAsync(p => p.Id == inventory.Partner.Id);

            var supplier = inventory.SupplierId.HasValue
                ? await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == inventory.SupplierId.Value)
                : null;

            return new InventoryDTO
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                ProductCode = product?.ProductCode,
                ProductName = product?.ProductName,
                QuantityInStock = inventory.QuantityInStock,
                UnitOfMeasure = product?.ConversionUnit, // Lấy từ Product
                InventoryValue = inventory.QuantityInStock * (product?.UnitPrice ?? 0) ?? 0,
                DateReceived = inventory.DateReceived,
                PartnerId = inventory.Partner.Id,
                SupplierId = inventory.SupplierId,
                SupplierName = supplier?.SupplierName,
                InventoryCode = inventory.InventoryCode,
                WarehouseLocation = inventory.WarehouseLocation,
                BatchNumber = inventory.BatchNumber,
                ExpirationDate = inventory.ExpirationDate,
                OrderQuantity = inventory.OrderQuantity ?? 0,
                MinimumStockLevel = inventory.MinimumStockLevel ?? 0,
                AvailableQuantity = inventory.AvailableQuantity,
                Brand = inventory.Brand,
                ReturnedQuantity = inventory.ReturnedQuantity
            };
        }

        public async Task<DataObjectResponse?> GenerateInventoryCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_context);

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
                new DataStringResponse(false, "Thông tin tổ chức không để trống !", null);


            var inventoryCode = await codeGenerator
            .GenerateNextCodeAsync<ProductInventory>(prefix: "TK",
                codeSelector: c => c.InventoryCode,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã tồn kho thành công", inventoryCode);
        }
        private async Task<InventoryDTO> GetInventoryByCode(string code, Partner partner)
        {
            var existingInventory = await _context.ProductInventories
                .FirstOrDefaultAsync(c => c.InventoryCode == code && c.PartnerId == partner.Id);
            if (existingInventory == null)
                return null;

            return new InventoryDTO
            {
                Id = existingInventory.Id,
                InventoryCode = existingInventory.InventoryCode,
                ProductName = existingInventory.ProductName
            };
        }
        public async Task<DataObjectResponse?> CheckInventoryCodeAsync(string code, Employee employee, Partner partner)
        {
            var inventoryDetail = await GetInventoryByCode(code, partner);

            if (inventoryDetail == null)
            {
                return new DataObjectResponse(true, "Mã tồn kho có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã tồn kho đã tồn tại", new
                {
                    inventoryDetail.Id,
                    inventoryDetail.InventoryCode,
                    inventoryDetail.ProductName
                });
            }
        }

    }
}
