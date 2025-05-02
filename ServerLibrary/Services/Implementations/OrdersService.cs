using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.MongoModels;
using Data.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class OrdersService : BaseService, IOrderService
    {
        private readonly AppDbContext _appContext;
        private readonly IMapper _mapper;
        private readonly IMongoCollection<OrderDetails> _ordersDetailsCollection;
        private readonly IProductInventoryService _productInventory;
        private readonly ICustomerService _customerService;

        public OrdersService(
            MongoDbContext dbContext,
            AppDbContext appContext,
            IProductInventoryService productInventory,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ICustomerService customerService
        )
            : base(appContext, httpContextAccessor)
        {
            _ordersDetailsCollection = dbContext.OrderDetails;
            _appContext = appContext;
            _mapper = mapper;
            _customerService = customerService;
            _productInventory = productInventory;
        }

        public async Task<List<OrderDTO>> GetFullOrdersByCustomerIdAsync(
            int customerId,
            Employee employee,
            Partner partner
        )
        {
            if (employee == null || !IsOwner)
            {
                throw new ArgumentNullException(
                    nameof(employee),
                    "Vui lòng không để trống ID Employee."
                );
            }
            if (partner == null || !IsOwner)
            {
                throw new ArgumentNullException(
                    nameof(partner),
                    "Vui lòng không để trống đối tác."
                );
            }
            try
            {
                IQueryable<Order> query = _appContext
                    .Orders.Where(o => o.Partner == partner && o.CustomerId == customerId)
                    .AsNoTracking();
                if (!IsOwner)
                {
                    query = query
                        .Where(o =>
                            o.OwnerId == employee.Id
                            || o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id)
                        )
                        .Include(o => o.OrderEmployees);
                }

                var orders = await query.ToListAsync();
                if (orders.Count == 0)
                    return new List<OrderDTO>();

                var orderIds = orders.Select(o => o.Id).Where(id => id != null).ToList();

                // 🔹 Truy vấn OrderDetails từ MongoDB
                var orderDetailsList = await _ordersDetailsCollection
                    .Find(d => orderIds.Contains(d.OrderId.Value))
                    .ToListAsync();

                var orderDetailsDict = orderDetailsList
                    .GroupBy(d => d.OrderId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // 🔹 Ánh xạ sang DTO
                var orderDtos = orders
                    .Select(order =>
                    {
                        var dto = _mapper.Map<OrderDTO>(order);
                        dto.OrderDetails = orderDetailsDict.TryGetValue(order.Id, out var details)
                            ? details
                                .Select(d => new OrderDetailDTO
                                {
                                    Id = d.Id,
                                    OrderId = d.OrderId,
                                    PartnerId = d.PartnerId,
                                    ProductId = d.ProductId ?? 0,
                                    CustomerId = d.CustomerId,
                                    Avatar = d.Avatar,
                                    CustomerName = d.CustomerName,
                                    PartnerName = d.PartnerName,
                                    SaleOrderNo = d.SaleOrderNo,
                                    ProductCode = d.ProductCode,
                                    ProductName = d.ProductName,
                                    TaxID = d.TaxID,
                                    TaxAmount = d.TaxAmount,
                                    TaxIDText = d.TaxIDText,
                                    DiscountRate = d.DiscountRate,
                                    DiscountAmount = d.DiscountAmount,
                                    InventoryItemID = d.InventoryItemID,
                                    UnitPrice = d.UnitPrice,
                                    QuantityInstock = d.QuantityInstock,
                                    Total = d.Total,
                                    UsageUnitID = d.UsageUnitID,
                                    UsageUnitIDText = d.UsageUnitIDText,
                                    Quantity = d.Quantity,
                                    AmountSummary = d.AmountSummary,
                                    CreatedAt = d.CreatedAt,
                                    UpdatedAt = d.UpdatedAt,
                                })
                                .ToList()
                            : new List<OrderDetailDTO>();

                        return dto;
                    })
                    .ToList();

                return orderDtos;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentException($"Lỗi tham số đầu vào: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<PagedResponse<List<OrderDTO>>> GetAllOrdersAsync(Employee employee, Partner partner, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Check null inputs
                if (employee == null)
                {
                    throw new ArgumentNullException(nameof(employee), "Vui lòng không để trống ID Employee.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Vui lòng không để trống đối tác.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10; // Default page size

                // Build query
                var query = _appContext.Orders
                    .Where(o => o.Partner.Id == partner.Id)
                    .AsNoTracking();

                if (!IsOwner)
                {
                    query = query
                        .Where(o =>
                            o.OwnerId == employee.Id ||
                            o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id))
                        .Include(o => o.OrderEmployees);
                }

                var totalRecords = await query.CountAsync();

                var orders = await query
                    .OrderBy(o => o.Id) // Add sorting for consistency
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs
                var orderDtos = _mapper.Map<List<OrderDTO>>(orders);

                return new PagedResponse<List<OrderDTO>>(
                    data: orderDtos ?? new List<OrderDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách đơn hàng thất bại: {ex.Message}", ex);
            }
        }
        public async Task<OrderDTO> GetOrderByIdAsync(int id, Employee employee, Partner partner)
        {
            try
            {
                if (employee == null)
                {
                    throw new ArgumentNullException(
                        nameof(employee),
                        "Vui lòng không để trống ID Nhân viên."
                    );
                }

                var order = await _appContext
                    .Orders.Where(o =>
                        o.Id == id
                        && (
                            o.Partner == partner && o.OwnerId == employee.Id
                            || o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id)
                        )
                    )
                    .Include(o => o.OrderEmployees)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    throw new KeyNotFoundException(
                        $"Đơn hàng của ID {id} không được tìm thấy trên nhân viên này."
                    );
                }

                var orderDetails = await _ordersDetailsCollection
                    .Find(d => d.OrderId.Value.ToString() == id.ToString())
                    .ToListAsync();

                var orderDto = _mapper.Map<OrderDTO>(order);
                orderDto.OrderDetails = orderDetails
                    .Select(d => new OrderDetailDTO
                    {
                        Id = d.Id,
                        OrderId = d.OrderId,
                        PartnerId = d.PartnerId,
                        ProductId = d.ProductId ?? 0,
                        Avatar = d.Avatar,
                        ProductCode = d.ProductCode,
                        ProductName = d.ProductName,
                        TaxID = d.TaxID,
                        TaxAmount = d.TaxAmount,
                        TaxIDText = d.TaxIDText,
                        DiscountRate = d.DiscountRate,
                        DiscountAmount = d.DiscountAmount,
                        UnitPrice = d.UnitPrice,
                        QuantityInstock = d.QuantityInstock,
                        InventoryItemID = d.InventoryItemID,
                        UsageUnitID = d.UsageUnitID,
                        Total = d.Total,
                        UsageUnitIDText = d.UsageUnitIDText,
                        Quantity = d.Quantity,
                        AmountSummary = d.AmountSummary,
                        CustomerId = d.CustomerId,
                        CustomerName = d.CustomerName,
                        CreatedAt = d.CreatedAt,
                        UpdatedAt = d.UpdatedAt,
                    })
                    .ToList();

                return orderDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> CreateOrderAsync(
            OrderDTO orderDto,
            Employee employee,
            Partner partner
        )
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    if (orderDto == null)
                    {
                        return new GeneralResponse(false, "Đơn hàng không hợp lệ.");
                    }
                    if (orderDto.OrderDetails == null)
                    {
                        return new GeneralResponse(false, "Hàng hoá không được để trống.");
                    }
                    var checkCodeExisted = await CheckOrderCodeAsync(orderDto.SaleOrderNo, employee, partner);

                    if (checkCodeExisted != null && !checkCodeExisted.Flag)
                        return new GeneralResponse(false, "Mã Đơn hàng đã tồn tại !");

                    var order = _mapper.Map<Order>(orderDto);
                    if (string.IsNullOrEmpty(order.SaleOrderNo))
                    {
                        order.SaleOrderNo = await codeGenerator.GenerateNextCodeAsync<Order>(
                            "ĐH",
                            c => c.SaleOrderNo,
                            c => c.Partner.Id == partner.Id
                        );
                    }
                    order.OwnerId = employee.Id;
                    order.Partner = partner;

                    _appContext.Orders.Add(order);
                    await _appContext.SaveChangesAsync();

                    
                    foreach (var detailDto in orderDto.OrderDetails)
                    {
                        var response = await _productInventory.CheckAndUpdateStockAsync(
                            detailDto.ProductId,
                            detailDto.Quantity
                        );
                        if (!response.Flag)
                        {   
                            await transaction.RollbackAsync();
                            return new GeneralResponse(false, response.Message);
                        }
                    }
                    var orderDetails = orderDto
                        .OrderDetails.Select(detailDto => new OrderDetails
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            OrderId = order.Id,
                            SaleOrderNo = order.SaleOrderNo,
                            PartnerId = partner.Id,
                            PartnerName = partner.Name,
                            Avatar = detailDto.Avatar,
                            // ** Order Detail Manually
                            ProductId = detailDto.ProductId,
                            ProductCode = detailDto.ProductCode,
                            ProductName = detailDto.ProductName,
                            InventoryItemID = detailDto.InventoryItemID,
                            TaxID = detailDto.TaxID,
                            TaxAmount = detailDto.TaxAmount,
                            TaxIDText = detailDto.TaxIDText,
                            DiscountRate = detailDto.DiscountRate,
                            DiscountAmount = detailDto.DiscountAmount,
                            UnitPrice = detailDto.UnitPrice,
                            Total = detailDto.Total,
                            QuantityInstock = detailDto.QuantityInstock,
                            UsageUnitID = detailDto.UsageUnitID,
                            UsageUnitIDText = detailDto.UsageUnitIDText,
                            Quantity = detailDto.Quantity,
                            AmountSummary = detailDto.AmountSummary,
                            CustomerId = detailDto.CustomerId,
                            CustomerName = detailDto.CustomerName,
                            CreatedAt = DateTime.Now,
                        })
                        .ToList();

                    // có orders mới chạy code
                    if (orderDetails.Any())
                    {
                        try
                        {
                            await _ordersDetailsCollection.InsertManyAsync(orderDetails);
                        }
                        catch (MongoDB.Driver.MongoConnectionException ex)
                        {
                            throw new Exception($"MongoDB connection failed: {ex.Message}", ex);
                        }
                    }

                    await transaction.CommitAsync();
                    return new GeneralResponse(
                        true,
                        $"Tạo đơn hàng thành công. Order ID: {order.Id}"
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(
                        false,
                        $"Lỗi khi lấy thông tin đơn hàng: {ex.Message}"
                    );
                }
            });
        }

        public async Task<GeneralResponse?> UpdateOrderAsync(
            int id,
            UpdateOrderDTO orderDTO,
            Employee employee,
            Partner partner
        )
        {
            var strategy = _appContext.Database.CreateExecutionStrategy();
            Console.WriteLine(
                $"Starting UpdateOrderAsync for Order ID: {id}, Employee ID: {employee?.Id}, Partner ID: {partner?.Id}"
            );
            var codeGenerator = new GenerateNextCode(_appDbContext);

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    Console.WriteLine("Fetching existing order from database...");
                    Order? existingOrder = null;
                    existingOrder = await _appContext
                        .Orders.Include(c => c.OrderEmployees)
                        .FirstOrDefaultAsync(o =>
                            o.Id == id && o.OwnerId == employee.Id
                            || o.OrderEmployees.Any(oe =>
                                oe.EmployeeId == employee.Id && oe.AccessLevel == AccessLevel.Write
                            )
                                && o.Partner == partner
                        );
                    if (existingOrder == null)
                    {
                        Console.WriteLine($"Order with ID {id} not found.");
                        return new GeneralResponse(false, "Không tìm thấy đơn hàng");
                    }

                    if (string.IsNullOrEmpty(existingOrder.SaleOrderNo))
                    {
                        string originalCode = existingOrder.SaleOrderNo;
                        bool exists = await _appDbContext.Orders.AnyAsync(c =>
             c.SaleOrderNo == existingOrder.SaleOrderNo &&
             c.PartnerId == partner.Id &&
             c.Id != id);
                        if (exists)
                        {
                            existingOrder.SaleOrderNo = await codeGenerator.GenerateNextCodeAsync<Order>("ĐH", c => c.SaleOrderNo, c => c.PartnerId == partner.Id);
                            Console.WriteLine($"SaleOrderNo '{originalCode}' already existed. Replaced with '{existingOrder.SaleOrderNo}' for SaleOrderNo ID {id}.");
                        }
                    }
                    Console.WriteLine(
                        $"Order found: ID {existingOrder.Id}, OwnerId {existingOrder.OwnerId}"
                    );
                    Console.WriteLine("Mapping orderDTO to existingOrder...");
                    _mapper.Map(orderDTO, existingOrder);

                    existingOrder.OwnerId = employee.Id;
                    existingOrder.Partner = partner;
                    Console.WriteLine("Updating order in database...");
                    _appContext.Orders.Update(existingOrder);
                    await _appContext.SaveChangesAsync();
                    Console.WriteLine("Order updated in SQL database successfully.");

                    var existingOrderDetails = await _ordersDetailsCollection
                        .Find(d => d.OrderId == id)
                        .ToListAsync();
                    Console.WriteLine(
                        $"Found {existingOrderDetails.Count} existing order details."
                    );
                    //  Prepare OrderDetails for MongoDB Update
                    var updatedOrderDetails = new List<ReplaceOneModel<OrderDetails>>();
                    var newOrderDetails = new List<OrderDetails>();
                    var detailIdsToKeep = new HashSet<string>();
                    Console.WriteLine("Processing order details...");
                    foreach (var detailDto in orderDTO.OrderDetails)
                    {
                        var detailId = string.IsNullOrEmpty(detailDto.Id)
                            ? ObjectId.GenerateNewId().ToString()
                            : detailDto.Id;
                        var existingDetail = existingOrderDetails.FirstOrDefault(d =>
                            d.Id == detailId
                        );

                        if (existingDetail != null)
                        {
                            existingDetail.ProductId = detailDto.ProductId;
                            existingDetail.ProductCode = detailDto.ProductCode;
                            existingDetail.ProductName = detailDto.ProductName;
                            existingDetail.TaxID = detailDto.TaxID;
                            existingDetail.TaxAmount = detailDto.TaxAmount;
                            existingDetail.TaxIDText = detailDto.TaxIDText;
                            existingDetail.DiscountRate = detailDto.DiscountRate;
                            existingDetail.DiscountAmount = detailDto.DiscountAmount;
                            existingDetail.UnitPrice = detailDto.UnitPrice;
                            existingDetail.QuantityInstock = detailDto.QuantityInstock;
                            existingDetail.InventoryItemID = detailDto.InventoryItemID;
                            existingDetail.UsageUnitID = detailDto.UsageUnitID;
                            existingDetail.UsageUnitIDText = detailDto.UsageUnitIDText;
                            existingDetail.Quantity = detailDto.Quantity;
                            existingDetail.AmountSummary = detailDto.AmountSummary;
                            existingDetail.Total = detailDto.Total;
                            existingDetail.CustomerId = detailDto.CustomerId;
                            existingDetail.CustomerName = detailDto.CustomerName;
                            updatedOrderDetails.Add(
                                new ReplaceOneModel<OrderDetails>(
                                    Builders<OrderDetails>.Filter.Eq(d => d.Id, detailId),
                                    existingDetail
                                )
                                {
                                    IsUpsert = true,
                                }
                            );
                        }
                        else
                        {
                            Console.WriteLine($"Creating new detail ID: {detailId}");
                            var newDetail = new OrderDetails
                            {
                                Id = detailId,
                                OrderId = existingOrder.Id,
                                SaleOrderNo = existingOrder.SaleOrderNo,
                                Avatar = detailDto.Avatar,
                                PartnerId = partner.Id,
                                PartnerName = existingOrder.Partner.Name,
                                ProductId = detailDto.ProductId,
                                InventoryItemID = detailDto.InventoryItemID,
                                ProductCode = detailDto.ProductCode,
                                ProductName = detailDto.ProductName,
                                TaxID = detailDto.TaxID,
                                TaxAmount = detailDto.TaxAmount,
                                TaxIDText = detailDto.TaxIDText,
                                DiscountRate = detailDto.DiscountRate,
                                DiscountAmount = detailDto.DiscountAmount,
                                UnitPrice = detailDto.UnitPrice,
                                QuantityInstock = detailDto.QuantityInstock,
                                UsageUnitID = detailDto.UsageUnitID,
                                UsageUnitIDText = detailDto.UsageUnitIDText,
                                Quantity = detailDto.Quantity,
                                Total = detailDto.Total,
                                AmountSummary = detailDto.AmountSummary,
                                CustomerId = detailDto.CustomerId,
                                CustomerName = detailDto.CustomerName,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now,
                            };
                            newOrderDetails.Add(newDetail);
                        }

                        detailIdsToKeep.Add(detailId);
                    }

                    var detailIdsToDelete = existingOrderDetails
                        .Select(d => d.Id)
                        .Except(detailIdsToKeep)
                        .ToList();
                    if (detailIdsToDelete.Any())
                    {
                        Console.WriteLine(
                            $"Deleting {detailIdsToDelete.Count} obsolete order details..."
                        );
                        await _ordersDetailsCollection.DeleteManyAsync(d =>
                            detailIdsToDelete.Contains(d.Id)
                        );
                        Console.WriteLine("Obsolete order details deleted.");
                    }

                    // Perform Bulk Update in MongoDB
                    if (updatedOrderDetails.Any())
                    {
                        Console.WriteLine(
                            $"Performing bulk update for {updatedOrderDetails.Count} order details..."
                        );
                        await _ordersDetailsCollection.BulkWriteAsync(updatedOrderDetails);
                        Console.WriteLine("Bulk update completed.");
                    }
                    if (newOrderDetails.Any())
                    {
                        Console.WriteLine(
                            $"Inserting {newOrderDetails.Count} new order details..."
                        );
                        await _ordersDetailsCollection.InsertManyAsync(newOrderDetails);
                        Console.WriteLine("New order details inserted.");
                    }
                    Console.WriteLine("Committing transaction...");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, "Cập nhật đơn hàng thành công");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(
                        false,
                        $"Lỗi khi lấy thông tin đơn hàng: {ex.Message}"
                    );
                }
            });
        }

        // public async Task<GeneralResponse?> RemoveOrderAsync(int orderId, int employeeId)
        // {
        //     try
        //     {
        //         if (orderId == null || employeeId == null)
        //         {
        //             return new GeneralResponse(false, "Order ID and Employee ID cannot be null or empty.");
        //         }
        //         var objectId = ObjectId.Parse(orderId.ToString());
        //         var existingOrder = await _ordersCollection.Find(o => o.Id == objectId.ToString()).FirstOrDefaultAsync();

        //         if (existingOrder == null)
        //         {
        //             return new GeneralResponse(false, "Order not found.");
        //         }
        //         // var hasAccess = existingOrder.EmployeeAccessLevels.Any(e => e.EmployeeId == employeeId && e.AccessLevel == AccessLevel.ReadWrite);

        //         // if (!hasAccess)
        //         // {
        //         //     return new GeneralResponse(false, "Access denied: Employee does not have readWrite access.");
        //         // }
        //         var deleteOrderResult = await _ordersCollection.DeleteOneAsync(o => o.Id == objectId.ToString());

        //         if (!deleteOrderResult.IsAcknowledged || deleteOrderResult.DeletedCount == 0)
        //         {
        //             return new GeneralResponse(false, $"Failed to delete order {orderId}.");
        //         }

        //         // Optionally delete associated OrderDetails
        //         var deleteDetailsResult = await _ordersDetailsCollection.DeleteManyAsync(d => d.OrderId == orderId);

        //         return new GeneralResponse(true, $"Order {orderId} and {deleteDetailsResult.DeletedCount} associated details removed successfully.");
        //     }
        //     catch (Exception ex)
        //     {
        //         return new GeneralResponse(false, $"Failed to delete order: {ex.Message}");
        //     }
        // }

        public async Task<GeneralResponse?> DeleteBulkOrdersAsync(
            string ids,
            Employee employee,
            Partner partner
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ids))
                {
                    return new GeneralResponse(false, "Mã hàng đơn không được để trống.");
                }

                if (employee == null)
                {
                    return new GeneralResponse(false, "Không tìm thấy nhân viên.");
                }

                var idList = ids.Split(',')
                    .Select(id => int.TryParse(id.Trim(), out int parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (!idList.Any())
                {
                    return new GeneralResponse(false, "Mã đơn hàng không hợp lệ.");
                }

                var ordersToDelete = await _appContext
                    .Orders.Where(o => idList.Contains(o.Id) && o.OwnerId == employee.Id)
                    .ToListAsync();

                if (!ordersToDelete.Any())
                {
                    throw new KeyNotFoundException("Không đơn hàng nào được xoá.");
                }

                var orderIdStrings = ordersToDelete.Select(o => o.Id.ToString()).ToList();

                await DeleteBulkOrderDetailsAsync(orderIdStrings);

                _appContext.Orders.RemoveRange(ordersToDelete);
                await _appContext.SaveChangesAsync();

                return new GeneralResponse(true, "Xoá đơn hàng thành công");
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo đơn hàng: {ex.Message}");
            }
        }

        private async Task<bool> DeleteBulkOrderDetailsAsync(List<string> orderIds)
        {
            try
            {
                if (orderIds == null || !orderIds.Any())
                {
                    throw new ArgumentException(
                        "Order ID list cannot be null or empty.",
                        nameof(orderIds)
                    );
                }

                var deleteResult = await _ordersDetailsCollection.DeleteManyAsync(d =>
                    orderIds.Contains(d.OrderId.ToString())
                );

                return deleteResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete order details: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> UpdateFieldIdAsync(
            int id,
            UpdateOrderDTO orderDTO,
            Employee employee,
            Partner partner
        )
        {
            var strategy = _appContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    if (orderDTO == null || id <= 0)
                    {
                        return new GeneralResponse(false, "Invalid order data provided.");
                    }

                    var existingOrder = await _appContext
                        .Orders.Include(c => c.OrderEmployees)
                        .FirstOrDefaultAsync(o =>
                            o.Id == id
                            && (
                                o.OwnerId == employee.Id
                                || o.OrderEmployees.Any(oe =>
                                    oe.EmployeeId == employee.Id
                                    && oe.AccessLevel == AccessLevel.Write
                                )
                            )
                            && o.Partner == partner
                        );

                    if (existingOrder == null)
                    {
                        return new GeneralResponse(false, "Order not found.");
                    }
                    decimal? previousSaleOrderAmount = existingOrder.SaleOrderAmount;

                    foreach (var prop in typeof(UpdateOrderDTO).GetProperties())
                    {
                        var newValue = prop.GetValue(orderDTO);
                        if (newValue != null)
                        {
                            var existingProp = typeof(Order).GetProperty(prop.Name);
                            if (existingProp != null)
                            {
                                var currentValue = existingProp.GetValue(existingOrder);
                                if (!object.Equals(currentValue, newValue))
                                {
                                    existingProp.SetValue(existingOrder, newValue);
                                    _appContext
                                        .Entry(existingOrder)
                                        .Property(existingProp.Name)
                                        .IsModified = true;
                                }
                            }
                        }
                    }
                    existingOrder.OwnerId = employee.Id;
                    existingOrder.Partner = partner;

                    await _appContext.SaveChangesAsync();

                    // Update MongoDB Order Details
                    if (orderDTO.OrderDetails != null)
                    {
                        var existingOrderDetails = await _ordersDetailsCollection
                            .Find(d => d.OrderId == id)
                            .ToListAsync();
                        var updatedOrderDetails = new List<ReplaceOneModel<OrderDetails>>();
                        var newOrderDetails = new List<OrderDetails>();
                        var detailIdsToKeep = new HashSet<string>();

                        foreach (var detailDto in orderDTO.OrderDetails)
                        {
                            var detailId = string.IsNullOrEmpty(detailDto.Id)
                                ? ObjectId.GenerateNewId().ToString()
                                : detailDto.Id;
                            var existingDetail = existingOrderDetails.FirstOrDefault(d =>
                                d.Id == detailId
                            );

                            if (existingDetail != null)
                            {
                                existingDetail = _mapper.Map<OrderDetails>(detailDto);
                                existingDetail.PartnerId = partner.Id;

                                updatedOrderDetails.Add(
                                    new ReplaceOneModel<OrderDetails>(
                                        Builders<OrderDetails>.Filter.Eq(d => d.Id, detailId),
                                        existingDetail
                                    )
                                    {
                                        IsUpsert = true,
                                    }
                                );
                            }
                            else
                            {
                                var newDetail = _mapper.Map<OrderDetails>(detailDto);
                                newDetail.Id = detailId;
                                newDetail.OrderId = existingOrder.Id;
                                newDetail.PartnerId = partner.Id;

                                newOrderDetails.Add(newDetail);
                            }

                            detailIdsToKeep.Add(detailId);
                        }

                        var detailIdsToDelete = existingOrderDetails
                            .Select(d => d.Id)
                            .Except(detailIdsToKeep)
                            .ToList();
                        if (detailIdsToDelete.Any())
                        {
                            await _ordersDetailsCollection.DeleteManyAsync(d =>
                                detailIdsToDelete.Contains(d.Id)
                            );
                        }

                        if (updatedOrderDetails.Any())
                        {
                            await _ordersDetailsCollection.BulkWriteAsync(updatedOrderDetails);
                        }
                        if (newOrderDetails.Any())
                        {
                            await _ordersDetailsCollection.InsertManyAsync(newOrderDetails);
                        }
                    }

                    await transaction.CommitAsync();
                    return new GeneralResponse(true, "Cập nhật đơn hàng thành công");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(
                        false,
                        $"Đã xảy ra lỗi khi cập nhật đơn hàng: {ex.Message}"
                    );
                }
            });
        }

        public async Task<OrderDTO> SimplyOrderDetailAsync(int id, Partner Partner)
        {
            try
            {
                var order = await _appContext.Orders
                 .Where(o =>
                     o.Id == id &&
                         o.PartnerId == Partner.Id
                 )
                 .FirstOrDefaultAsync();

                if (order == null)
                {
                    throw new KeyNotFoundException($"Đơn hàng ID {id} không tồn tại");
                }
                var orderDto = _mapper.Map<OrderDTO>(order);
                return orderDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi check đơn hàng: {ex.Message}.");
            }
        }
        public async Task<GeneralResponse?> BulkAddContactsIntoOrder(
            List<int> ContactIds,
            int id,
            Employee employee,
            Partner Partner
        )
        {
            if (ContactIds == null || !ContactIds.Any())
                return new GeneralResponse(false, "Danh sách liên hệ không được để trống!");

            var order = await SimplyOrderDetailAsync(id, Partner);

            if (order == null)
                return new GeneralResponse(false, "Không tìm thấy đơn hàng!");

            var contacts = await _appDbContext
                .Contacts.Where(c => ContactIds.Contains(c.Id) && c.PartnerId == Partner.Id)
                .ToListAsync();

            if (!contacts.Any())
                return new GeneralResponse(false, "Không tìm thấy liên hệ !");

            var existingContactIds = _appContext
                .OrderContacts.Select(oc => oc.ContactId)
                .ToHashSet();
            var newOrderContacts = contacts
                .Where(c => !existingContactIds.Contains(c.Id))
                .Select(c => new OrderContacts
                {
                    OrderId = order.Id,
                    ContactId = c.Id,
                    PartnerId = Partner.Id,
                })
                .ToList();

            if (!newOrderContacts.Any())
                return new GeneralResponse(false, "Tất cả liên hệ đã có trong đơn hàng!");

            _appDbContext.OrderContacts.AddRange(newOrderContacts);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Thêm liên hệ vào đơn hàng thành công!");
        }

        public async Task<List<Activity?>> GetAllActivitiesByOrderAsync(
            int orderId,
            Employee employee,
            Partner partner
        )
        {
            if (employee == null || partner == null)
            {
                throw new ArgumentNullException(
                    nameof(employee),
                    "ID nhân viên và ID tổ chức không đuọc bỏ trống."
                );
            }
            var activities = await _appDbContext
                .Activities
                // .Include(c => c.ActivityEmployees)
                .Where(c =>
                    c.OrderId == orderId
                    && c.TaskOwnerId == employee.Id
                    && c.PartnerId == partner.Id
                )
                .ToListAsync();

            return activities.Any() ? activities : new List<Activity>();
        }

        public async Task<PagedResponse<List<ContactDTO>>> GetAllContactsAvailableByIdAsync(
            int id,
            Employee employee,
            Partner partner,
            int pageNumber,
            int pageSize
        )
        {
            try
            {
                // Check inputs
                if (id <= 0)
                {
                    throw new ArgumentException("ID đơn hàng phải lớn hơn 0.");
                }
                if (employee == null)
                {
                    throw new ArgumentNullException(nameof(employee), "Vui lòng không để trống ID Employee.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Thông tin tổ chức không được để trống.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var query = _appDbContext.Contacts
                    .Where(c => !c.OrderContacts.Any(cc => cc.OrderId == id && cc.PartnerId == partner.Id))
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                var contacts = await query
                    .OrderBy(c => c.Id) // Add sorting for consistency
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var contactDtos = _mapper.Map<List<ContactDTO>>(contacts);

                return new PagedResponse<List<ContactDTO>>(
                    data: contactDtos ?? new List<ContactDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách liên hệ thất bại: {ex.Message}", ex);
            }
        }

        public async Task<List<ContactDTO>> GetAllContactsLinkedIdAsync(
            int id,
            Employee employee,
            Partner partner
        )
        {
            if (id == null)
            {
                throw new ArgumentException("ID khách hàng không được để trống !");
            }
            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }

            var result = await _appDbContext
                .Contacts.Where(c =>
                    c.OrderContacts.Any(cc => cc.OrderId == id && cc.PartnerId == partner.Id)
                )
                .Select(c => new ContactDTO
                {
                    Id = c.Id,
                    ContactCode = c.ContactCode,
                    ContactName = c.ContactName,
                    FullName = $"{c.LastName} {c.FirstName}",
                    SalutationID = c.SalutationID,
                    OfficeEmail = c.OfficeEmail,
                    TitleID = c.TitleID,
                    Mobile = c.Mobile,
                    Email = c.Email,
                })
                .ToListAsync();
            return result;
        }

        public async Task<GeneralResponse?> RemoveInvoiceFromIdAsync(
            int id,
            Employee employee,
            Partner partner
        )
        {
            Console.WriteLine(
                $"Starting RemoveInvoiceFromIdAsync for Order ID: {id}, Employee ID: {employee?.Id}, Partner ID: {partner?.Id}"
            );

            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Execution strategy started.");
                using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    // Tìm các InvoiceOrders liên quan đến OrderId và PartnerId
                    Console.WriteLine($"Fetching InvoiceOrders for Order ID {id}...");
                    var invoiceOrders = await _appContext
                        .InvoiceOrders.Where(io => io.OrderId == id && io.PartnerId == partner.Id)
                        .ToListAsync();

                    if (!invoiceOrders.Any())
                    {
                        Console.WriteLine($"No InvoiceOrders found for Order ID {id}.");
                        return new GeneralResponse(
                            true,
                            $"Đơn hàng ID {id} không liên kết với hóa đơn nào."
                        );
                    }

                    Console.WriteLine(
                        $"Removing {invoiceOrders.Count} InvoiceOrders for Order ID {id}..."
                    );
                    _appContext.InvoiceOrders.RemoveRange(invoiceOrders);
                    await _appContext.SaveChangesAsync();
                    Console.WriteLine("InvoiceOrders removed successfully.");

                    Console.WriteLine("Committing transaction...");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(
                        true,
                        $"Đã xóa liên kết hóa đơn khỏi đơn hàng ID {id}."
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(
                        false,
                        $"Không thể xóa liên kết hóa đơn khỏi đơn hàng: {ex.Message}"
                    );
                }
            });
        }

        public async Task<List<InvoiceDTO>> GetAllInvoicesAsync(
            int id,
            Employee employee,
            Partner partner
        )
        {
            if (id == null)
            {
                throw new ArgumentException("ID khách hàng không được để trống !");
            }
            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }
            try
            {
                Console.WriteLine($"Begin getting invoices related to order id {id}");
                var invoices = await _appDbContext
                    .Invoices.Where(c =>
                        c.InvoiceOrders.Any(cc => cc.OrderId == id && cc.PartnerId == partner.Id)
                    )
                    .ToListAsync();
                var orderDtos = _mapper.Map<List<InvoiceDTO>>(invoices);
                Console.WriteLine($"Successfully query invoices, count {orderDtos.Count} invoices");
                if (orderDtos.Count == 0)
                {
                    return new List<InvoiceDTO>();
                }
                return orderDtos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There are error when query from db, {ex.Message}");
                throw new Exception($"Failed to delete invoice details: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> UnassignCustomerFromOrder(
            int id,
            int customerId,
            Employee employee,
            Partner partner
        )
        {
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");
                    var order = await _appContext.Orders.FirstOrDefaultAsync(o =>
                        o.Id == id && o.CustomerId == customerId && o.Partner.Id == partner.Id
                    );
                    if (order == null)
                    {
                        Console.WriteLine($"No order found for ID {customerId}.");
                        return new GeneralResponse(
                            false,
                            $"Không tìm thấy đơn hàng với ID {customerId}."
                        );
                    }

                    order.CustomerId = null;
                    _appContext.Orders.Update(order);
                    await _appContext.SaveChangesAsync();
                    Console.WriteLine("Order unassigned from customer successfully.");
                    await transaction.CommitAsync();
                    return new GeneralResponse(
                        true,
                        $"Đã gỡ bỏ liên kết đơn hàng với khách hàng ID {id}."
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(
                        false,
                        $"Không thể gỡ bỏ liên kết đơn hàng với khách hàng: {ex.Message}"
                    );
                }
            });
        }

        public async Task<GeneralResponse?> UnassignActivityFromOrder(
            int id,
            int activityId,
            Partner partner
        )
        {
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");
                    var activity = await _appDbContext.Activities.FirstOrDefaultAsync(a =>
                        a.Id == activityId && a.OrderId == id && a.PartnerId == partner.Id
                    );
                    if (activity == null)
                    {
                        Console.WriteLine($"No activity found for ID {activityId}.");
                        return new GeneralResponse(
                            false,
                            $"Không tìm thấy hoạt động với ID {activityId}."
                        );
                    }

                    activity.OrderId = null;
                    _appDbContext.Activities.Update(activity);
                    await _appDbContext.SaveChangesAsync();
                    Console.WriteLine("Activity unassigned from order successfully.");
                    await transaction.CommitAsync();
                    return new GeneralResponse(
                        true,
                        $"Đã gỡ bỏ liên kết hoạt động với đơn hàng ID {id}."
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(
                        false,
                        $"Không thể gỡ bỏ liên kết hoạt động với đơn hàng: {ex.Message}"
                    );
                }
            });
        }

        public async Task<GeneralResponse?> RemoveContactFromOrder(
            int id,
            int contactId,
            Employee employee,
            Partner partner
        )
        {
            var strategy = _appContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _appContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");
                    var order = await _appDbContext.OrderContacts.FirstOrDefaultAsync(c =>
                        c.ContactId == contactId && c.OrderId == id && c.PartnerId == partner.Id
                    );
                    if (order == null)
                    {
                        Console.WriteLine($"No order found for ID {contactId}.");
                        return new GeneralResponse(
                            false,
                            $"Không tìm thấy đơn hàng với ID {contactId}."
                        );
                    }
                    _appDbContext.OrderContacts.Remove(order);
                    await _appDbContext.SaveChangesAsync();
                    Console.WriteLine("Contact unassigned from order successfully.");
                    await transaction.CommitAsync();
                    return new GeneralResponse(
                        true,
                        $"Đã gỡ bỏ liên kết liên hệ với đơn hàng ID {id}."
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new GeneralResponse(
                        false,
                        $"Không thể gỡ bỏ liên kết liên hệ với đơn hàng: {ex.Message}"
                    );
                }
            });
        }

        private TimeSpan? CalculatePurchaseCycle(List<OrderDTO> orders)
        {
            if (orders == null || !orders.Any())
            {
                return null;
            }

            var orderedDates = orders
                .OrderBy(o => o.SaleOrderDate)
                .Select(o => o.SaleOrderDate)
                .ToList();
            if (orderedDates.Count < 2)
            {
                return null;
            }

            var intervals = new List<TimeSpan>();
            for (int i = 1; i < orderedDates.Count; i++)
            {
                intervals.Add(orderedDates[i] - orderedDates[i - 1]);
            }

            return TimeSpan.FromTicks((long)intervals.Average(ts => ts.Ticks));
        }

        public CustomerCycleDTO GetPurchaseCycleDTO(List<OrderDTO> orders)
        {
            var ts = CalculatePurchaseCycle(orders);
            if (ts == null)
                return null;

            return new CustomerCycleDTO
            {
                PurchaseCycleDays = Math.Round(ts.Value.TotalDays, 1),
                ReadablePurchaseCycle =
                    $"{(int)ts.Value.TotalDays} ngày, {ts.Value.Hours}h {ts.Value.Minutes}m",
            };
        }

        public async Task<OrderStats> GetOrderStatsForCustomer(
            int customerId,
            Employee employee,
            Partner partner
        )
        {
            var orderCustomer = await GetFullOrdersByCustomerIdAsync(customerId, employee, partner);

            if (orderCustomer == null || !orderCustomer.Any())
            {
                return new OrderStats();
            }
            var stats = new OrderStats
            {
                OrderCount = orderCustomer.Count(),
                TotalOrderValue = orderCustomer.Sum(o => o.SaleOrderAmount),
                Debt = orderCustomer.Where(o => o.IsPaid == false).Sum(o => o.SaleOrderAmount),
                LastPurchaseDate = orderCustomer.Max(o => o.SaleOrderDate),
                PurchaseCycle = GetPurchaseCycleDTO(orderCustomer),
                PurchasedItems = orderCustomer.SelectMany(o => o.OrderDetails).ToList(),
            };
            return stats;
        }

        private async Task<OptionalOrderDTO> GetOrderByCode(string code, Partner partner)
        {
            var existingOrder = await _appDbContext.Orders
                .FirstOrDefaultAsync(c => c.SaleOrderNo == code && c.PartnerId == partner.Id);
            if (existingOrder == null)
                return null;

            return new OptionalOrderDTO
            {
                Id = existingOrder.Id,
                SaleOrderNo = existingOrder.SaleOrderNo,
                SaleOrderName = existingOrder.SaleOrderName,
            };
        }

        public async Task<DataObjectResponse?> GenerateOrderCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            var orderCode = await codeGenerator
            .GenerateNextCodeAsync<Order>(prefix: "ĐH",
                codeSelector: c => c.SaleOrderNo,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã đơn hàng thành công", orderCode);
        }

        public async Task<DataObjectResponse?> CheckOrderCodeAsync(string code, Employee employee, Partner partner)
        {
            var orderDetail = await GetOrderByCode(code, partner);

            if (orderDetail == null)
            {
                return new DataObjectResponse(true, "Mã đơn hàng có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã đơn hàng đã tồn tại", new
                {
                    orderDetail.SaleOrderNo,
                    orderDetail.SaleOrderName,
                    orderDetail.Id
                });
            }
        }

        public async Task<List<OrderDTO>> GetOrdersStatsCalculation(Employee employee, Partner partner)
        {
            {
                if (employee == null || !IsOwner)
                {
                    throw new ArgumentNullException(nameof(employee), "Vui lòng không để trống ID Employee.");
                }
                if (partner == null || !IsOwner)
                {
                    throw new ArgumentNullException(nameof(partner), "Vui lòng không để trống đối tác.");
                }
                try
                {
                    IQueryable<Order> query = _appContext.Orders
                        .Where(o => o.Partner == partner)
                        .AsNoTracking();
                    if (!IsOwner)
                    {
                        query = query.Where(o =>
                            o.OwnerId == employee.Id ||
                            o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id))
                            .Include(o => o.OrderEmployees);
                    }

                    var orders = await query.ToListAsync();
                    if (orders.Count == 0) return new List<OrderDTO>();

                    var orderIds = orders.Select(o => o.Id).Where(id => id != null).ToList();

                    // 🔹 Truy vấn OrderDetails từ MongoDB
                    var orderDetailsList = await _ordersDetailsCollection
                        .Find(d => orderIds.Contains(d.OrderId.Value))
                        .ToListAsync();


                    var orderDetailsDict = orderDetailsList
                        .GroupBy(d => d.OrderId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    // 🔹 Ánh xạ sang DTO
                    var orderDtos = orders.Select(order =>
                    {
                        var dto = _mapper.Map<OrderDTO>(order);
                        dto.OrderDetails = orderDetailsDict.TryGetValue(order.Id, out var details)
                            ? details.Select(d => new OrderDetailDTO
                            {
                                Id = d.Id,
                                OrderId = d.OrderId,
                                PartnerId = d.PartnerId,
                                ProductId = d.ProductId ?? 0,
                                CustomerId = d.CustomerId,
                                CustomerName = d.CustomerName,
                                PartnerName = d.PartnerName,
                                SaleOrderNo = d.SaleOrderNo,
                                ProductCode = d.ProductCode,
                                ProductName = d.ProductName,
                                TaxID = d.TaxID,
                                TaxAmount = d.TaxAmount,
                                TaxIDText = d.TaxIDText,
                                DiscountRate = d.DiscountRate,
                                DiscountAmount = d.DiscountAmount,
                                UnitPrice = d.UnitPrice,
                                QuantityInstock = d.QuantityInstock,
                                Total = d.Total,
                                UsageUnitID = d.UsageUnitID,
                                UsageUnitIDText = d.UsageUnitIDText,
                                Quantity = d.Quantity,
                                AmountSummary = d.AmountSummary,
                                CreatedAt = d.CreatedAt,
                                UpdatedAt = d.UpdatedAt
                            }).ToList()
                            : new List<OrderDetailDTO>();

                        return dto;
                    }).ToList();

                    return orderDtos;
                }
                catch (ArgumentNullException ex)
                {
                    throw new ArgumentException($"Lỗi tham số đầu vào: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}", ex);
                }
            }
        }
    }
}
