

using Data.Enums;

namespace Data.DTOs
{


    public class UpdateInventoryDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; } // Khóa ngoại liên kết với Product

        public string? ProductCode { get; set; } // Mã hàng hóa (Product Code/ID) - Đồng bộ với Product

        public string? ProductName { get; set; } // Tên hàng hóa (Product Name) - Đồng bộ với Product

        public int? QuantityInStock { get; set; } // Số lượng tồn kho (Quantity in Stock)

        public string? UnitOfMeasure { get; set; } // Đơn vị tính (Unit of Measure) - Có thể lấy từ Product.ConversionUnit

        public string? WarehouseLocation { get; set; } // Vị trí kho (Warehouse/Location)

        public decimal InventoryValue { get; set; } // Giá trị tồn kho (Inventory Value) - Tính từ UnitCost * QuantityInStock

        public DateTime? DateReceived { get; set; } // Ngày nhập kho (Date Received)

        public DateTime? DateDispatched { get; set; } // Ngày xuất kho (Date Dispatched)

        public StockStatus? StockStatus { get; set; } // Trạng thái hàng hóa (Stock Status): "In Stock", "Out of Stock", "Pending", v.v.

        public string? BatchNumber { get; set; } // Số lô (Batch/Lot Number)

        public DateTime? ExpirationDate { get; set; } // Hạn sử dụng (Expiration Date)

        public int OrderQuantity { get; set; } // Số lượng đặt hàng (Order Quantity)

        public int? AvailableQuantity { get; set; } // Số lượng có sẵn (Available Quantity)

        public int? ReturnedQuantity { get; set; } // Số lượng trả hàng (Returned Quantity)

        public string? SupplierName { get; set; } // Tên nhà cung cấp (Supplier Name)

        public string? Brand { get; set; } // Thương hiệu (Brand)

        public int MinimumStockLevel { get; set; } // Ngưỡng tồn kho tối thiểu (Minimum Stock Level)
        public int PartnerId { get; set; }
        public int? SupplierId { get; set; }
    }
}