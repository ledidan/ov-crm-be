using MongoDB.Driver;
using Data.MongoModels;
using ServerLibrary.Data;
using Data.Enums;
using Data.Responses;
using Data.DTOs.Order;
using MongoDB.Bson;

namespace ServerLibrary.Services.Implementations
{
    public class OrdersService
    {
        private readonly IMongoCollection<Orders> _ordersCollection;

        private readonly IMongoCollection<OrderDetails> _ordersDetailsCollection;
        public OrdersService(MongoDbContext dbContext)
        {
            _ordersCollection = dbContext.Orders;
            _ordersDetailsCollection = dbContext.OrderDetails;
        }

        public async Task<List<Orders>> GetAllAsync(int employeeId, int partnerId)
        {
            var orders = Builders<Orders>.Filter.And(Builders<Orders>.Filter.Eq(o => o.PartnerId, partnerId),
               Builders<Orders>.Filter.ElemMatch(o => o.EmployeeAccessLevels,
            access => access.EmployeeId == employeeId)
              );
            return await _ordersCollection.Find(orders).ToListAsync();
        }

        public async Task<Orders> GetOrderDetailAsync(string orderId, int employeeId)
        {
            try
            {
                if (string.IsNullOrEmpty(orderId) || employeeId <= 0)
                {
                    throw new KeyNotFoundException($"Employee ID or order ID must not be empty.");
                }

                var objectId = ObjectId.Parse(orderId);

                var orderFilter = Builders<Orders>.Filter.Eq(o => o.Id, objectId.ToString());
                var order = await _ordersCollection.Find(orderFilter).FirstOrDefaultAsync();

                if (order == null)
                {
                    throw new KeyNotFoundException($"Order ID  not found.");
                }

                var hasAccess = order.EmployeeAccessLevels.Any(e => e.EmployeeId == employeeId);

                if (!hasAccess)
                {
                    throw new ArgumentException($"Access denied: Employee ID {employeeId} does not have access to this order.");
                }

                // Fetch the associated OrderDetails
                var orderDetailsFilter = Builders<OrderDetails>.Filter.Eq(od => od.OrderId, orderId);
                var orderDetails = await _ordersDetailsCollection.Find(orderDetailsFilter).ToListAsync();

                // Attach order details to the order object
                order.OrderDetails = orderDetails;

                return order;
            }
            catch (Exception ex)
            {
               throw new ArgumentException($"Failed to retrieve order details: {ex.Message}");
            }
        }

        public async Task<GeneralResponse> CreateOrderAsync(OrderDTO orderDto, List<OrderDetailDTO> orderDetailsDto)
        {
            try
            {
                if (orderDto == null || orderDetailsDto == null || orderDetailsDto.Count == 0)
                {
                    return new GeneralResponse(false, "Invalid order or order details.");
                }
                var order = new Orders
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    OrderCode = orderDto.OrderCode,
                    TotalAmount = orderDto.TotalAmount,
                    IsPaid = orderDto.IsPaid,
                    IsShared = orderDto.IsShared,
                    OrderDate = orderDto.OrderDate,
                    PaidDate = orderDto.PaidDate,
                    CustomerId = orderDto.CustomerId,
                    PartnerId = orderDto.PartnerId,
                    ContactId = orderDto.ContactId,
                    EmployeeAccessLevels = orderDto.EmployeeAccessLevels
                        .Select(e => new EmployeeAccess
                        {
                            EmployeeId = e.EmployeeId,
                            AccessLevel = (AccessLevel)e.AccessLevel
                        })
                        .ToList()
                };

                await _ordersCollection.InsertOneAsync(order);
                var orderDetails = orderDetailsDto.Select(detailDto => new OrderDetails
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    OrderId = order.Id,
                    ProductId = detailDto.ProductId,
                    Quantity = detailDto.Quantity,
                    SellingPrice = detailDto.SellingPrice,
                    Amount = detailDto.Quantity * detailDto.SellingPrice,
                }).ToList();
                await _ordersDetailsCollection.InsertManyAsync(orderDetails);
                return new GeneralResponse(true, $"Order created successfully. Order ID: {order.Id}");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"Failed to create order: {ex.Message}");
            }
        }

        public async Task<GeneralResponse> UpdateOrderAsync(string orderId, OrderDTO orderDTO, List<OrderDetailDTO> orderDetailDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(orderId) || orderDTO == null || orderDetailDTO == null || orderDetailDTO.Count == 0)
                {
                    return new GeneralResponse(false, "Invalid order ID, order data, or order details.");
                }

                var objectId = ObjectId.Parse(orderId);
                var existingOrder = await _ordersCollection.Find(o => o.Id == objectId.ToString()).FirstOrDefaultAsync();

                if (existingOrder == null)
                {
                    return new GeneralResponse(false, "Order not found.");
                }

                existingOrder.OrderCode = orderDTO.OrderCode;
                existingOrder.TotalAmount = orderDTO.TotalAmount;
                existingOrder.IsPaid = orderDTO.IsPaid;
                existingOrder.IsShared = orderDTO.IsShared;
                existingOrder.OrderDate = orderDTO.OrderDate;
                existingOrder.PaidDate = orderDTO.PaidDate;
                existingOrder.CustomerId = orderDTO.CustomerId;
                existingOrder.PartnerId = orderDTO.PartnerId;
                existingOrder.ContactId = orderDTO.ContactId;
                existingOrder.EmployeeAccessLevels = orderDTO.EmployeeAccessLevels
                    .Select(e => new EmployeeAccess
                    {
                        EmployeeId = e.EmployeeId,
                        AccessLevel = (AccessLevel)e.AccessLevel
                    })
                    .ToList();
                var orderUpdateResult = await _ordersCollection.ReplaceOneAsync(o => o.Id == objectId.ToString(), existingOrder);

                if (!orderUpdateResult.IsAcknowledged || orderUpdateResult.ModifiedCount == 0)
                {
                    return new GeneralResponse(false, $"Failed to update order {orderId}");
                }
                var existingOrderDetails = await _ordersDetailsCollection.Find(d => d.OrderId == orderId).ToListAsync();

                var updatedOrderDetails = orderDetailDTO.Select(detailDto =>
                {
                    var detailId = string.IsNullOrEmpty(detailDto.Id) ? ObjectId.GenerateNewId().ToString() : detailDto.Id;

                    var existingDetail = existingOrderDetails.FirstOrDefault(d => d.Id == detailId);
                    if (existingDetail != null)
                    {
                        existingDetail.ProductId = detailDto.ProductId;
                        existingDetail.Quantity = detailDto.Quantity;
                        existingDetail.SellingPrice = detailDto.SellingPrice;
                        existingDetail.Amount = detailDto.Quantity * detailDto.SellingPrice;
                        return existingDetail;
                    }

                    return new OrderDetails
                    {
                        Id = detailId,
                        OrderId = orderId,
                        ProductId = detailDto.ProductId,
                        Quantity = detailDto.Quantity,
                        SellingPrice = detailDto.SellingPrice,
                        Amount = detailDto.Quantity * detailDto.SellingPrice,
                    };
                }).ToList();
                var detailIdsToKeep = updatedOrderDetails.Select(d => d.Id).ToHashSet();
                var existingDetailIds = existingOrderDetails.Select(d => d.Id).ToHashSet();
                var detailIdsToDelete = existingDetailIds.Except(detailIdsToKeep).ToList();
                if (detailIdsToDelete.Any())
                {
                    await _ordersDetailsCollection.DeleteManyAsync(d => detailIdsToDelete.Contains(d.Id));
                }
                // Insert or update OrderDetails
                await _ordersDetailsCollection.BulkWriteAsync(updatedOrderDetails.Select(detail =>
                    new ReplaceOneModel<OrderDetails>(
                        Builders<OrderDetails>.Filter.Eq(d => d.Id, detail.Id),
                        detail
                    )
                    { IsUpsert = true }
                ));

                return new GeneralResponse(true, "Order updated successfully.");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"Failed to update order: {ex.Message}\n{ex.StackTrace}");
            }
        }
        public async Task<GeneralResponse> RemoveOrderAsync(string orderId, int employeeId)
        {
            try
            {
                if (string.IsNullOrEmpty(orderId) || employeeId == null)
                {
                    return new GeneralResponse(false, "Order ID and Employee ID cannot be null or empty.");
                }

                var objectId = ObjectId.Parse(orderId);
                var existingOrder = await _ordersCollection.Find(o => o.Id == objectId.ToString()).FirstOrDefaultAsync();

                if (existingOrder == null)
                {
                    return new GeneralResponse(false, "Order not found.");
                }
                var hasAccess = existingOrder.EmployeeAccessLevels.Any(e => e.EmployeeId == employeeId && e.AccessLevel == AccessLevel.ReadWrite);

                if (!hasAccess)
                {
                    return new GeneralResponse(false, "Access denied: Employee does not have readWrite access.");
                }
                var deleteOrderResult = await _ordersCollection.DeleteOneAsync(o => o.Id == objectId.ToString());

                if (!deleteOrderResult.IsAcknowledged || deleteOrderResult.DeletedCount == 0)
                {
                    return new GeneralResponse(false, $"Failed to delete order {orderId}.");
                }

                // Optionally delete associated OrderDetails
                var deleteDetailsResult = await _ordersDetailsCollection.DeleteManyAsync(d => d.OrderId == orderId);

                return new GeneralResponse(true, $"Order {orderId} and {deleteDetailsResult.DeletedCount} associated details removed successfully.");
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, $"Failed to delete order: {ex.Message}");
            }
        }

        // This is example insert order
        public async Task InsertSampleOrderAsync()
        {
            var sampleOrder = new Orders
            {
                TotalAmount = 140.75,
                IsPaid = true,
                IsShared = false,
                PaidDate = DateTime.UtcNow,
                CustomerId = 1,
                PartnerId = 1,
                ContactId = 2,
                EmployeeAccessLevels = new List<EmployeeAccess>
        {
            new EmployeeAccess { EmployeeId = 2, AccessLevel = AccessLevel.ReadWrite }
        }

            };
            await _ordersCollection.InsertOneAsync(sampleOrder);
        }
    }
}
