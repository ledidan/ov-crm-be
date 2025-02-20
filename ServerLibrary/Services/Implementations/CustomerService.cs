using System.Runtime.ConstrainedExecution;
using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        private readonly IContactService _contactService;

        private readonly IPartnerService _partnerService;
        private readonly IEmployeeService _employeeService;
        public CustomerService(AppDbContext appDbContext,
            IPartnerService partnerService,
            IContactService contactService,
             IEmployeeService employeeService, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _partnerService = partnerService;
            _contactService = contactService;
            _employeeService = employeeService;
            _mapper = mapper;
        }
        public async Task<GeneralResponse> CreateAsync(CreateCustomer customer, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            if (customer == null) return new GeneralResponse(false, "Model is empty");

            // Check partner
            if (partner == null) return new GeneralResponse(false, "Partner not found");

            // Check employee   
            if (employee == null)
                return new GeneralResponse(false, "Employee not found");

            if (string.IsNullOrEmpty(customer.AccountNumber))
            {
                customer.AccountNumber = await codeGenerator.GenerateNextCodeAsync<Customer>("KH", c => c.AccountNumber);
            }
            var newCustomer = new Customer
            {
                AccountName = customer.AccountName,
                AccountNumber = customer.AccountNumber,
                AccountReferredID = customer.AccountReferredID,
                AccountShortName = customer.AccountShortName,
                AccountTypeID = customer.AccountTypeID,
                Avatar = customer.Avatar,
                BankAccount = customer.BankAccount,
                BankName = customer.BankName,
                BillingCode = customer.BillingCode,
                BillingCountryID = customer.BillingCountryID,
                BillingDistrictID = customer.BillingDistrictID,
                BillingLat = customer.BillingLat,
                BillingLong = customer.BillingLong,
                BillingProvinceID = customer.BillingProvinceID,
                BillingStreet = customer.BillingStreet,
                BillingWardID = customer.BillingWardID,
                BudgetCode = customer.BudgetCode,
                RevenueDetail = customer.RevenueDetail,
                BusinessTypeID = customer.BusinessTypeID,
                ContactIDAim = customer.ContactIDAim,
                NumberOfDaysOwed = customer.NumberOfDaysOwed,
                OwnerID = customer.EmployeeId.ToString(),
                Fax = customer.Fax,
                GenderID = customer.GenderID,
                Identification = customer.Identification,
                IndustryID = customer.IndustryID,
                Inactive = customer.Inactive,
                Latitude = customer.Latitude,
                LeadSourceID = customer.LeadSourceID,
                NoOfEmployeeID = customer.NoOfEmployeeID,
                OfficeEmail = customer.OfficeEmail,
                OfficeTel = customer.OfficeTel,
                OrganizationUnitID = customer.OrganizationUnitID,
                SectorText = customer.SectorText,
                ShippingCode = customer.ShippingCode,
                ShippingCountryID = customer.ShippingCountryID,
                ShippingDistrictID = customer.ShippingDistrictID,
                ShippingLat = customer.ShippingLat,
                ShippingLong = customer.ShippingLong,
                ShippingProvinceID = customer.ShippingProvinceID,
                AnnualRevenueID = customer.AnnualRevenueID,
                ShippingStreet = customer.ShippingStreet,
                ShippingWardID = customer.ShippingWardID,
                TaxCode = customer.TaxCode,
                Website = customer.Website,
                CelebrateDate = customer.CelebrateDate,
                Debt = customer.Debt,
                DebtLimit = customer.DebtLimit,
                Description = customer.Description,

                // Boolean Fields
                IsPublic = customer.IsPublic,
                IsPartner = customer.IsPartner,
                IsPersonal = customer.IsPersonal,
                IsOldCustomer = customer.IsOldCustomer,
                IsDistributor = customer.IsDistributor,

                Partner = partner,
                Employee = employee,
                CustomerSinceDate = customer.CustomerSinceDate
            };

            newCustomer.CustomerEmployees.Add(new CustomerEmployees
            {
                Employee = employee,
                EmployeeId = employee.Id,
                Partner = partner,
                PartnerId = partner.Id,
                AccessLevel = AccessLevel.ReadWrite
            });

            await _appDbContext.InsertIntoDb(newCustomer);
            return new GeneralResponse(true, "Customer created");
        }


        public async Task<GeneralResponse> DeleteAsync(int customerId,
       Employee employee, Partner partner)
        {
            var customer = await _appDbContext.Customers.Include(c => c.CustomerEmployees)
            .Where(c => c.PartnerId == partner.Id)
            .FirstOrDefaultAsync(ce => ce.Id == customerId);
            if (customer == null)
            {
                return new GeneralResponse(false, "Customer not found ");
            }
            var employeeExists = customer.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.AccessLevel == AccessLevel.ReadWrite);

            if (!employeeExists)
            {
                return new GeneralResponse(false, $"Employee with ID {employee.Id} does not have permission to remove this customer.");
            }

            _appDbContext.Customers.Remove(customer);

            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Removed customer successfully");
        }

        public async Task<GeneralResponse?> DeleteBulkCustomers(string ids, Employee employee, Partner partner)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return new GeneralResponse(false, "No customers provided for deletion");
            }

            // Convert comma-separated string into a List<int>
            var idList = ids.Split(',')
                            .Select(id => int.TryParse(id, out int parsedId) ? parsedId : (int?)null)
                            .Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .ToList();

            if (!idList.Any())
            {
                return new GeneralResponse(false, "Invalid customer IDs provided");
            }

            // Fetch customers that match the provided IDs and belong to the given partner
            var customers = await _appDbContext.Customers
                .Include(c => c.CustomerEmployees)
                .Where(c => idList.Contains(c.Id) && c.PartnerId == partner.Id)
                .ToListAsync();

            if (!customers.Any())
            {
                return new GeneralResponse(false, "Customers not found");
            }

            // Check if the employee has the required permission to delete each customer
            var unauthorizedCustomers = customers
                .Where(c => !c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.AccessLevel == AccessLevel.ReadWrite))
                .ToList();

            if (unauthorizedCustomers.Any())
            {
                return new GeneralResponse(false, "You are not authorized to delete some customers");
            }

            // Remove the valid customers
            _appDbContext.Customers.RemoveRange(customers);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Customers deleted successfully");
        }
        public async Task<List<Customer?>> GetAllAsync(Employee employee, Partner partner)
        {

            var employeeData = await _employeeService.FindByIdAsync(employee.Id);
            if (employeeData == null)
            {
                throw new ArgumentException($"Employee with ID {employee.Id} does not exist.");
            }
            var result = await _appDbContext.Customers
       .Include(c => c.CustomerEmployees)
       .Where(c => c.PartnerId == partner.Id
                   && c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id))
       .ToListAsync();

            return result.Any() ? result : new List<Customer>();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id, Employee employee, Partner partner)
        {
            var customer = await _appDbContext.Customers
                .Include(c => c.CustomerEmployees)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.Employee.PartnerId == partner.Id));

            if (customer == null)
            {
                return null;
            }
            return customer;
        }
        public async Task<CustomerDTO?> UpdateAsync(int id, CustomerDTO updateDto, Employee employee, Partner partner)
        {
            var existingCustomer = await _appDbContext.Customers
                            .Include(c => c.CustomerEmployees).AsNoTracking()
                            .FirstOrDefaultAsync(c =>
                                c.Id == id &&
                                c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.Employee.PartnerId == partner.Id));
            if (existingCustomer == null)
                return null;
            _mapper.Map(updateDto, existingCustomer);
            await _appDbContext.UpdateDb(existingCustomer);
            return _mapper.Map<CustomerDTO>(existingCustomer);
        }

        public async Task<GeneralResponse?> UpdateFieldIdAsync(int id, CustomerDTO updateCustomer, Employee employee, Partner partner)
        {
            var existingCustomer = await _appDbContext.Customers
                    .Include(c => c.CustomerEmployees)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id &&
                        c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.Employee.PartnerId == partner.Id));

            if (existingCustomer == null)
                return new GeneralResponse(false, "Customer not found");

            _mapper.Map(updateCustomer, existingCustomer);

            _appDbContext.Entry(existingCustomer).State = EntityState.Modified;

            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Customer updated successfully");
        }

    }
}
