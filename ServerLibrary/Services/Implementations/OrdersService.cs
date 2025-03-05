using MongoDB.Driver;
using Data.MongoModels;
using ServerLibrary.Data;
using Data.Enums;
using Data.Responses;
using MongoDB.Bson;
using Data.Entities;
using ServerLibrary.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Data.DTOs;
using Microsoft.AspNetCore.Http;

namespace ServerLibrary.Services.Implementations
{
    public class OrdersService : BaseService, IOrderService
    {
        private readonly AppDbContext _appContext;
        private readonly IMapper _mapper;
        private readonly IMongoCollection<OrderDetails> _ordersDetailsCollection;
        public OrdersService(MongoDbContext dbContext, AppDbContext appContext, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(appContext, httpContextAccessor)
        {
            _ordersDetailsCollection = dbContext.OrderDetails;
            _appContext = appContext;
            _mapper = mapper;
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync(Employee employee, Partner partner)
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
                            ProductId = d.ProductId,
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
                            AmountSummary = d.AmountSummary
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


        public async Task<OrderDTO> GetOrderByIdAsync(int id, Employee employee, Partner partner)
        {
            try
            {
                if (employee == null)
                {
                    throw new ArgumentNullException(nameof(employee), "Vui lòng không để trống ID Nhân viên.");
                }

                var order = await _appContext.Orders
                    .Where(o => o.Id == id &&
                                (o.Partner == partner && o.OwnerId == employee.Id ||
                                 o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id)))
                    .Include(o => o.OrderEmployees)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    throw new KeyNotFoundException($"Đơn hàng của ID {id} không được tìm thấy trên nhân viên này.");
                }

                var orderDetails = await _ordersDetailsCollection
                    .Find(d => d.OrderId.Value.ToString() == id.ToString())
                    .ToListAsync();

                var orderDto = _mapper.Map<OrderDTO>(order);
                orderDto.OrderDetails = orderDetails.Select(d => new OrderDetailDTO
                {
                    Id = d.Id,
                    OrderId = d.OrderId,
                    PartnerId = d.PartnerId,
                    ProductId = d.ProductId,
                    ProductCode = d.ProductCode,
                    ProductName = d.ProductName,
                    TaxID = d.TaxID,
                    TaxAmount = d.TaxAmount,
                    TaxIDText = d.TaxIDText,
                    DiscountRate = d.DiscountRate,
                    DiscountAmount = d.DiscountAmount,
                    UnitPrice = d.UnitPrice,
                    QuantityInstock = d.QuantityInstock,
                    UsageUnitID = d.UsageUnitID,
                    Total = d.Total,
                    UsageUnitIDText = d.UsageUnitIDText,
                    Quantity = d.Quantity,
                    AmountSummary = d.AmountSummary
                }).ToList();

                return orderDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy thông tin đơn hàng: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> CreateOrderAsync(OrderDTO orderDto,
         Employee employee, Partner partner)
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
                var order = _mapper.Map<Order>(orderDto);

                order.OwnerId = employee.Id;
                order.Partner = partner;

                _appContext.Orders.Add(order);
                await _appContext.SaveChangesAsync();

                // Create OrderDetails in MongoDB
                var orderDetails = orderDto.OrderDetails.Select(detailDto => new OrderDetails
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    OrderId = order.Id,
                    PartnerId = partner.Id,
                    ProductId = detailDto.ProductId,
                    ProductCode = detailDto.ProductCode,
                    ProductName = detailDto.ProductName,
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
                }).ToList();
                await _ordersDetailsCollection.InsertManyAsync(orderDetails);

                await transaction.CommitAsync();
                return new GeneralResponse(true, $"Tạo đơn hàng thành công. Order ID: {order.Id}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse(false, $"Lỗi khi lấy thông tin đơn hàng: {ex.Message}");
            }
        }
        public async Task<GeneralResponse?> UpdateOrderAsync(int id, OrderDTO orderDTO,
        Employee employee, Partner partner)
        {
            using var transaction = await _appContext.Database.BeginTransactionAsync();
            try
            {
                Order? existingOrder = null;
                existingOrder = await _appContext.Orders.Include(c => c.OrderEmployees).FirstOrDefaultAsync(o => o.Id == id
              && o.OwnerId == employee.Id || o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id && oe.AccessLevel == AccessLevel.Write)
              && o.Partner == partner);

                if (existingOrder == null)
                {
                    return new GeneralResponse(false, "Không tìm thấy đơn hàng");
                }

                _mapper.Map(orderDTO, existingOrder);


                existingOrder.OwnerId = employee.Id;
                existingOrder.Partner = partner;

                _appContext.Orders.Update(existingOrder);
                await _appContext.SaveChangesAsync();


                var existingOrderDetails = await _ordersDetailsCollection.Find(d => d.OrderId == id).ToListAsync();

                //  Prepare OrderDetails for MongoDB Update
                var updatedOrderDetails = new List<ReplaceOneModel<OrderDetails>>();
                var newOrderDetails = new List<OrderDetails>();
                var detailIdsToKeep = new HashSet<string>();

                foreach (var detailDto in orderDTO.OrderDetails)
                {
                    var detailId = string.IsNullOrEmpty(detailDto.Id) ? ObjectId.GenerateNewId().ToString() : detailDto.Id;
                    var existingDetail = existingOrderDetails.FirstOrDefault(d => d.Id == detailId);

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
                        existingDetail.UsageUnitID = detailDto.UsageUnitID;
                        existingDetail.UsageUnitIDText = detailDto.UsageUnitIDText;
                        existingDetail.Quantity = detailDto.Quantity;
                        existingDetail.AmountSummary = detailDto.AmountSummary;
                        existingDetail.Total = detailDto.Total;
                        existingDetail.PartnerId = partner.Id;

                        updatedOrderDetails.Add(new ReplaceOneModel<OrderDetails>(
                            Builders<OrderDetails>.Filter.Eq(d => d.Id, detailId),
                            existingDetail
                        )
                        { IsUpsert = true });
                    }
                    else
                    {
                        var newDetail = new OrderDetails
                        {
                            Id = detailId,
                            OrderId = existingOrder.Id,
                            PartnerId = partner.Id,
                            ProductId = detailDto.ProductId,
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
                            AmountSummary = detailDto.AmountSummary
                        };
                        newOrderDetails.Add(newDetail);
                    }

                    detailIdsToKeep.Add(detailId);
                }


                var detailIdsToDelete = existingOrderDetails.Select(d => d.Id).Except(detailIdsToKeep).ToList();
                if (detailIdsToDelete.Any())
                {
                    await _ordersDetailsCollection.DeleteManyAsync(d => detailIdsToDelete.Contains(d.Id));
                }

                // Perform Bulk Update in MongoDB
                if (updatedOrderDetails.Any())
                {
                    await _ordersDetailsCollection.BulkWriteAsync(updatedOrderDetails);
                }
                if (newOrderDetails.Any())
                {
                    await _ordersDetailsCollection.InsertManyAsync(newOrderDetails);
                }


                await transaction.CommitAsync();
                return new GeneralResponse(true, "Cập nhật đơn hàng thành công");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse(false, $"Lỗi khi lấy thông tin đơn hàng: {ex.Message}");
            }
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

        public async Task<GeneralResponse?> DeleteBulkOrdersAsync(string ids, Employee employee, Partner partner)
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

                var ordersToDelete = await _appContext.Orders
                    .Where(o => idList.Contains(o.Id) && o.OwnerId == employee.Id)
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
                    throw new ArgumentException("Order ID list cannot be null or empty.", nameof(orderIds));
                }

                var deleteResult = await _ordersDetailsCollection.DeleteManyAsync(d => orderIds.Contains(d.OrderId.ToString()));

                return deleteResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete order details: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> UpdateFieldIdAsync(int id, OrderDTO orderDTO, Employee employee, Partner partner)
        {

            using var transaction = await _appContext.Database.BeginTransactionAsync();
            try
            {
                if (orderDTO == null || id <= 0)
                {
                    return new GeneralResponse(false, "Invalid order data provided.");
                }

                Order? existingOrder = await _appContext.Orders
                    .Include(c => c.OrderEmployees)
                    .FirstOrDefaultAsync(o => o.Id == id
                        && (o.OwnerId == employee.Id || o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id && oe.AccessLevel == AccessLevel.Write))
                        && o.Partner == partner);

                if (existingOrder == null)
                {
                    return new GeneralResponse(false, "Order not found.");
                }

                _mapper.Map(orderDTO, existingOrder);

                existingOrder.OwnerId = employee.Id;
                existingOrder.Partner = partner;

                _appContext.Orders.Update(existingOrder);
                await _appContext.SaveChangesAsync();

                // Update MongoDB Order Details
                var existingOrderDetails = await _ordersDetailsCollection.Find(d => d.OrderId == id).ToListAsync();

                var updatedOrderDetails = new List<ReplaceOneModel<OrderDetails>>();
                var newOrderDetails = new List<OrderDetails>();
                var detailIdsToKeep = new HashSet<string>();

                foreach (var detailDto in orderDTO.OrderDetails)
                {
                    var detailId = string.IsNullOrEmpty(detailDto.Id) ? ObjectId.GenerateNewId().ToString() : detailDto.Id;
                    var existingDetail = existingOrderDetails.FirstOrDefault(d => d.Id == detailId);

                    if (existingDetail != null)
                    {
                        // Overwrite all fields in existing detail
                        _mapper.Map(detailDto, existingDetail);
                        existingDetail.PartnerId = partner.Id;

                        updatedOrderDetails.Add(new ReplaceOneModel<OrderDetails>(
                            Builders<OrderDetails>.Filter.Eq(d => d.Id, detailId),
                            existingDetail
                        )
                        { IsUpsert = true });
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

                var detailIdsToDelete = existingOrderDetails.Select(d => d.Id).Except(detailIdsToKeep).ToList();
                if (detailIdsToDelete.Any())
                {
                    await _ordersDetailsCollection.DeleteManyAsync(d => detailIdsToDelete.Contains(d.Id));
                }

                // Perform Bulk Update in MongoDB
                if (updatedOrderDetails.Any())
                {
                    await _ordersDetailsCollection.BulkWriteAsync(updatedOrderDetails);
                }
                if (newOrderDetails.Any())
                {
                    await _ordersDetailsCollection.InsertManyAsync(newOrderDetails);
                }

                await transaction.CommitAsync();
                return new GeneralResponse(true, "Order updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse(false, $"Failed to update order: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> BulkUpdateOrdersAsync(List<int> orderIds, int? ContactId, int? CustomerId, Employee employee, Partner partner)
        {
            using var transaction = await _appContext.Database.BeginTransactionAsync();
            try
            {
                // Get all orders that match the provided Order IDs and belong to the employee/partner
                var ordersToUpdate = await _appContext.Orders
                    .Where(o => orderIds.Contains(o.Id)
                                && (o.OwnerId == employee.Id
                                    || o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id && oe.AccessLevel == AccessLevel.Write))
                                && o.Partner == partner)
                    .ToListAsync();

                if (!ordersToUpdate.Any())
                {
                    return new GeneralResponse(false, "Đơn hàng không được tìm thấy.");
                }

                foreach (var order in ordersToUpdate)
                {
                    if (ContactId.HasValue)
                        if (ContactId.Value == 0)
                            order.ContactId = null;
                        else
                            order.ContactId = ContactId.Value;

                    if (CustomerId.HasValue)
                        if (CustomerId.Value == 0)
                            order.CustomerId = null;
                        else
                            order.CustomerId = CustomerId.Value;
                }

                _appContext.Orders.UpdateRange(ordersToUpdate);
                await _appContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return new GeneralResponse(true, $"Cập nhật thành công {ordersToUpdate.Count} đơn hàng.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new GeneralResponse(false, $"Cập nhật đơn hàng không thành công: {ex.Message}");
            }
        }
    }
}
