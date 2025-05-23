using Microsoft.EntityFrameworkCore;
using Moq;
using MongoDB.Driver;
using AutoMapper;
using Data.DTOs;
using Data.Entities;
using MongoDB.Bson;
using ServerLibrary.Data;
using Data.MongoModels;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Implementations;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Tests.Services
{
    public class InvoiceServiceTests
    {
        private readonly Mock<MongoDbContext> _mongoDbContextMock;
        private readonly Mock<AppDbContext> _appDbContextMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<GenerateNextCode> _codeGeneratorMock;
        private readonly Mock<IMongoCollection<OrderDetails>> _orderDetailsCollectionMock;
        private readonly Mock<IMongoCollection<InvoiceDetails>> _invoicesDetailsCollectionMock;
        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            _mongoDbContextMock = new Mock<MongoDbContext>();
            _appDbContextMock = new Mock<AppDbContext>();
            _mapperMock = new Mock<IMapper>();
            _codeGeneratorMock = new Mock<GenerateNextCode>();
            _orderDetailsCollectionMock = new Mock<IMongoCollection<OrderDetails>>();
            _invoicesDetailsCollectionMock = new Mock<IMongoCollection<InvoiceDetails>>();

            _mongoDbContextMock.Setup(m => m.OrderDetails).Returns(_orderDetailsCollectionMock.Object);
            _mongoDbContextMock.Setup(m => m.InvoiceDetails).Returns(_invoicesDetailsCollectionMock.Object);

            _invoiceService = new InvoiceService(_mongoDbContextMock.Object, _appDbContextMock.Object, _mapperMock.Object);
        }


        [Fact]
        public async Task CreateInvoiceFromOrdersAsync_ValidOrderIds_ReturnsSuccess()
        {
            // Arrange
            var orderIds = new List<int> { 1, 2 };
            var partner = new Partner { Id = 1, Name = "Partner1" };
            var employee = new Employee { Id = 1, EmployeeCode = "123123", Partner = partner };
            var orders = new List<Order>
            {
                new Order {  Partner = partner, Id = 1, CustomerId = 1, CustomerName = "Customer1", SaleOrderAmount = 1000, BillingCode = "BC1", BillingCountryID = "VN", BillingProvinceID = "HCM", BillingDistrictID = "D1", BillingStreet = "123 Street", ShippingReceivingPerson = "John", InvoiceReceivingEmail = "john@example.com", InvoiceReceivingPhone = "123456789" },
                new Order { Partner = partner, Id = 2, CustomerId = 1, CustomerName = "Customer1", SaleOrderAmount = 2000, BillingCode = "BC1", BillingCountryID = "VN", BillingProvinceID = "HCM", BillingDistrictID = "D1", BillingStreet = "123 Street", ShippingReceivingPerson = "John", InvoiceReceivingEmail = "john@example.com", InvoiceReceivingPhone = "123456789" }
            };
            var orderDetails = new List<OrderDetailDTO>
            {
                new OrderDetailDTO { OrderId = 1, ProductId = 101, ProductCode = "P1", ProductName = "Product1", Quantity = 2, UnitPrice = 500, Total = 1000, AmountSummary = 1000, CustomerId = 1, CustomerName = "Customer1", SaleOrderNo = "SO1", PartnerId = 1, PartnerName = "Partner1" },
                new OrderDetailDTO { OrderId = 2, ProductId = 102, ProductCode = "P2", ProductName = "Product2", Quantity = 1, UnitPrice = 2000, Total = 2000, AmountSummary = 2000, CustomerId = 1, CustomerName = "Customer1", SaleOrderNo = "SO2", PartnerId = 1, PartnerName = "Partner1" }
            };
            var invoice = new Invoice { Partner = partner, Id = 1, InvoiceRequestName = "HD001" };
            var invoiceDto = new InvoiceDTO { Id = 1, InvoiceRequestName = "HD001", CustomerId = 1, CustomerName = "Customer1", TotalSummary = 3000, AmountSummary = 3000 };

            var ordersDbSetMock = CreateDbSetMock(orders);
            var invoiceOrdersDbSetMock = CreateDbSetMock(new List<InvoiceOrders>());
            _appDbContextMock.Setup(db => db.Orders).Returns(ordersDbSetMock.Object);
            _appDbContextMock.Setup(db => db.InvoiceOrders).Returns(invoiceOrdersDbSetMock.Object);
            _appDbContextMock.Setup(db => db.Invoices.Add(It.IsAny<Invoice>())).Callback<Invoice>(i => i.Id = 1);
            _appDbContextMock.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);
            _appDbContextMock.Setup(db => db.Database.CreateExecutionStrategy()).Returns(new Mock<IExecutionStrategy>().Object);
            _appDbContextMock.Setup(db => db.Database.BeginTransactionAsync(default)).ReturnsAsync(new Mock<IDbContextTransaction>().Object);

            var orderDetailsCursorMock = CreateAsyncCursorMock(orderDetails);
            _orderDetailsCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<OrderDetails>>(), It.IsAny<FindOptions<OrderDetails, OrderDetailDTO>>(), default))
                .ReturnsAsync(orderDetailsCursorMock.Object);

            _invoicesDetailsCollectionMock.Setup(c => c.InsertManyAsync(It.IsAny<IEnumerable<InvoiceDetails>>(), null, default)).Returns(Task.CompletedTask);

            _codeGeneratorMock.Setup(c => c.GenerateNextCodeAsync<Invoice>("HĐ",
             It.IsAny<Expression<Func<Invoice, string>>>(), It.IsAny<Expression<Func<Invoice, bool>>>())).ReturnsAsync("HD001");
            _mapperMock.Setup(m => m.Map<Invoice>(It.IsAny<InvoiceDTO>())).Returns(invoice);

            // Act
            var result = await _invoiceService.CreateInvoiceFromOrdersAsync(orderIds, employee, partner);

            // Assert
            Assert.True(result.Flag);
            Assert.Equal($"Hóa đơn được tạo thành công từ {orders.Count} đơn hàng. Mã hóa đơn: {invoice.Id}", result.Message);
            _appDbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Exactly(2)); // For Invoices and Orders/InvoiceOrders
            _invoicesDetailsCollectionMock.Verify(c => c.InsertManyAsync(It.IsAny<IEnumerable<InvoiceDetails>>(), null, default), Times.Once());
        }

        [Fact]
        public async Task CreateInvoiceFromOrdersAsync_EmptyOrderIds_ReturnsError()
        {
            // Arrange
            var orderIds = new List<int>();
            var partner = new Partner { Id = 1 };
            var employee = new Employee { Id = 1, Partner = partner, EmployeeCode = " 12323" };

            // Act
            var result = await _invoiceService.CreateInvoiceFromOrdersAsync(orderIds, employee, partner);

            // Assert
            Assert.False(result.Flag);
            Assert.Equal("Danh sách đơn hàng không hợp lệ.", result.Message);
            _appDbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Never());
            _invoicesDetailsCollectionMock.Verify(c => c.InsertManyAsync(It.IsAny<IEnumerable<InvoiceDetails>>(), null, default), Times.Never());
        }

        [Fact]
        public async Task CreateInvoiceFromOrdersAsync_NoOrdersFound_ReturnsError()
        {
            // Arrange
            var orderIds = new List<int> { 1, 2 };
            var partner = new Partner { Id = 1 };
            var employee = new Employee { Id = 1, Partner = partner, EmployeeCode = " 12323" };
            var orders = new List<Order>();

            var ordersDbSetMock = CreateDbSetMock(orders);
            _appDbContextMock.Setup(db => db.Orders).Returns(ordersDbSetMock.Object);

            // Act
            var result = await _invoiceService.CreateInvoiceFromOrdersAsync(orderIds, employee, partner);

            // Assert
            Assert.False(result.Flag);
            Assert.Equal("Không tìm thấy đơn hàng hợp lệ.", result.Message);
            _appDbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Never());
            _invoicesDetailsCollectionMock.Verify(c => c.InsertManyAsync(It.IsAny<IEnumerable<InvoiceDetails>>(), null, default), Times.Never());
        }

        [Fact]
        public async Task CreateInvoiceFromOrdersAsync_NoOrderDetailsFound_ReturnsError()
        {
            // Arrange
            var orderIds = new List<int> { 1, 2 };
            var partner = new Partner { Id = 1 };
            var employee = new Employee { Id = 1, Partner = partner, EmployeeCode = " 12323" };
            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerId = 1, CustomerName = "Customer1", SaleOrderAmount = 1000, Partner = partner }
            };
            var orderDetails = new List<OrderDetailDTO>();

            var ordersDbSetMock = CreateDbSetMock(orders);
            _appDbContextMock.Setup(db => db.Orders).Returns(ordersDbSetMock.Object);
            var orderDetailsCursorMock = CreateAsyncCursorMock(orderDetails);
            _orderDetailsCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<OrderDetails>>(), It.IsAny<FindOptions<OrderDetails, OrderDetailDTO>>(), default))
                .ReturnsAsync(orderDetailsCursorMock.Object);

            // Act
            var result = await _invoiceService.CreateInvoiceFromOrdersAsync(orderIds, employee, partner);

            // Assert
            Assert.False(result.Flag);
            Assert.Equal("Không tìm thấy chi tiết đơn hàng.", result.Message);
            _appDbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Never());
            _invoicesDetailsCollectionMock.Verify(c => c.InsertManyAsync(It.IsAny<IEnumerable<InvoiceDetails>>(), null, default), Times.Never());
        }

        [Fact]
        public async Task CreateInvoiceFromOrdersAsync_MultipleCustomerIds_ReturnsError()
        {
            // Arrange
            var orderIds = new List<int> { 1, 2 };
            var partner = new Partner { Id = 1 };
            var employee = new Employee { Id = 1, Partner = partner, EmployeeCode = " 12323" };
            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerId = 1, CustomerName = "Customer1", SaleOrderAmount = 1000 , Partner = partner},
                new Order { Id = 2, CustomerId = 2, CustomerName = "Customer2", SaleOrderAmount = 2000, Partner = partner }
            };

            var ordersDbSetMock = CreateDbSetMock(orders);
            _appDbContextMock.Setup(db => db.Orders).Returns(ordersDbSetMock.Object);

            // Act
            var result = await _invoiceService.CreateInvoiceFromOrdersAsync(orderIds, employee, partner);

            // Assert
            Assert.False(result.Flag);
            Assert.Equal("Các đơn hàng thuộc nhiều khách hàng khác nhau, không thể tạo hóa đơn chung.", result.Message);
            _appDbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Never());
            _invoicesDetailsCollectionMock.Verify(c => c.InsertManyAsync(It.IsAny<IEnumerable<InvoiceDetails>>(), null, default), Times.Never());
        }

        [Fact]
        public async Task CreateInvoiceFromOrdersAsync_ExceptionThrown_ReturnsError()
        {
            // Arrange
            var orderIds = new List<int> { 1 };
            var partner = new Partner { Id = 1 };
            var employee = new Employee { Id = 1, Partner = partner, EmployeeCode = "4232342" };
            var orders = new List<Order>
            {
                new Order { Id = 1, CustomerId = 1, CustomerName = "Customer1", SaleOrderAmount = 1000, Partner = partner }
            };

            var ordersDbSetMock = CreateDbSetMock(orders);
            _appDbContextMock.Setup(db => db.Orders).Returns(ordersDbSetMock.Object);
            _appDbContextMock.Setup(db => db.Database.CreateExecutionStrategy()).Throws(new Exception("Database error"));

            // Act
            var result = await _invoiceService.CreateInvoiceFromOrdersAsync(orderIds, employee, partner);

            // Assert
            Assert.False(result.Flag);
            Assert.StartsWith("Không thể tạo hóa đơn: Database error", result.Message);
            _appDbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Never());
            _invoicesDetailsCollectionMock.Verify(c => c.InsertManyAsync(It.IsAny<IEnumerable<InvoiceDetails>>(), null, default), Times.Never());
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(IEnumerable<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return dbSetMock;
        }

        private static Mock<IAsyncCursor<T>> CreateAsyncCursorMock<T>(IEnumerable<T> data)
        {
            var cursorMock = new Mock<IAsyncCursor<T>>();
            cursorMock.Setup(c => c.Current).Returns(data);
            cursorMock.SetupSequence(c => c.MoveNextAsync(default))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            return cursorMock;
        }
    }
}