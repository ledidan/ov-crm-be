using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;
using ServerLibrary.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Data.Entities;
using Data.DTOs;
using Data.ThirdPartyModels;
using Data.Responses;


namespace Server.Tests
{
    public class CustomerServiceTests
    {
        private readonly AppDbContext _appDbContext;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IPartnerService> _partnerServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private CustomerService _customerService;
        private readonly IImportLogger _importLogger;
        private readonly Mock<IImportLogger> _importLoggerMock;

        public CustomerServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _appDbContext = new AppDbContext(options);
            _importLoggerMock = new Mock<IImportLogger>() ?? throw new ArgumentNullException(nameof(_importLogger)); ;
            _partnerServiceMock = new Mock<IPartnerService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mapperMock = new Mock<IMapper>();

            var fakeHttpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(_ => _.HttpContext).Returns(fakeHttpContext);

            _customerService = new CustomerService(
                _appDbContext,
                _partnerServiceMock.Object,
                _mapperMock.Object,
                _importLoggerMock.Object,
                _httpContextAccessorMock.Object
            );

        }

        private Partner CreateFakePartner() => new Partner { Id = 1, Name = "TestPartner" };
        private Employee CreateFakeEmployee() => new Employee
        {
            Id = 1,
            FullName = "TestEmployee",
            EmployeeCode = "213213",
            Partner = new Partner
            {
                Id = 1,
                Name = "TestPartner"
            }
        };


        [Fact]
        public async Task ImportCustomerDataAsync_ShouldSaveErrorLogUrl_WhenErrorsExist()
        {
            // Arrange
            var partner = CreateFakePartner();
            var employee = CreateFakeEmployee();
            var errorCustomer = new CustomerDTO { AccountNumber = null };

            // Act
            var result = await _customerService.ImportCustomerDataAsync(
                new List<CustomerDTO> { errorCustomer },
                employee,
                partner
            );

            // Assert
            Assert.Single(result.Errors);
            Assert.Equal("mocked_path/customer_import_errors.json", result.ErrorLogUrl);
            _importLoggerMock.Verify(x => x.SaveImportErrorsToFile(It.IsAny<List<ImportError<CustomerDTO>>>(), "customer_import_errors"), Times.Once);
        }
    }

}
