using System.Runtime.ConstrainedExecution;
using AutoMapper;
using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class CustomerService : BaseService, ICustomerService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        private readonly IContactService _contactService;

        private readonly IPartnerService _partnerService;
        private readonly IEmployeeService _employeeService;
        public CustomerService(AppDbContext appDbContext,
            IPartnerService partnerService,
            IContactService contactService,
             IEmployeeService employeeService, IMapper mapper,
             IHttpContextAccessor httpContextAccessor
             ) : base(appDbContext, httpContextAccessor)
        {
            _appDbContext = appDbContext;
            _partnerService = partnerService;
            _contactService = contactService;
            _employeeService = employeeService;
            _mapper = mapper;
        }



        public async Task<GeneralResponse?> BulkAddContactsIntoCustomer(List<int> contactIds, int customerId, Employee employee, Partner partner)
        {
            if (contactIds == null || !contactIds.Any())
                return new GeneralResponse(false, "Danh sách liên hệ không được để trống!");

            var customer = await GetCustomerByIdAsync(customerId, employee, partner);

            if (customer == null)
                return new GeneralResponse(false, "Không tìm thấy khách hàng!");

            var contacts = await _appDbContext.Contacts
                .Where(c => contactIds.Contains(c.Id) && c.PartnerId == partner.Id)
                .ToListAsync();

            if (!contacts.Any())
                return new GeneralResponse(false, "Không tìm thấy liên hệ !");

            var existingContactIds = customer.CustomerContacts.Select(oc => oc.ContactId).ToHashSet();
            var newCustomerContacts = contacts
                .Where(c => !existingContactIds.Contains(c.Id))
                .Select(c => new CustomerContacts
                {
                    CustomerId = customer.Id,
                    ContactId = c.Id,
                    PartnerId = partner.Id
                })
                .ToList();

            if (!newCustomerContacts.Any())
                return new GeneralResponse(false, "Liên hệ đã liên kết với khách hàng !");

            _appDbContext.CustomerContacts.AddRange(newCustomerContacts);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Thêm liên hệ vào thông tin khách hàng thành công!");
        }

        public async Task<DataStringResponse> CreateAsync(CreateCustomer customer, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            if (customer == null) return new DataStringResponse(false, "Thông tin khách hàng rỗng !");

            // Check partner
            if (partner == null) return new DataStringResponse(false, "Thông tin tổ chức không để trống !");

            // Check employee   
            if (employee == null)
                return new DataStringResponse(false, "Không tìm thấy nhân viên");

            if (string.IsNullOrEmpty(customer.AccountNumber))
            {
                customer.AccountNumber = await codeGenerator.GenerateNextCodeAsync<Customer>("KH", c => c.AccountNumber, c => c.PartnerId == partner.Id);
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
            return new DataStringResponse(true, "Tạo khách hàng thành công !", newCustomer.Id.ToString());
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
            var employeeExists = customer.CustomerEmployees
            .Any(ce => ce.EmployeeId == employee.Id && ce.AccessLevel == AccessLevel.ReadWrite);

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
                .Where(c => !c.CustomerEmployees
                .Any(ce => ce.EmployeeId == employee.Id && ce.AccessLevel == AccessLevel.ReadWrite))
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

        public async Task<List<Activity>> GetAllActivitiesByIdAsync(int id, Partner partner)
        {
            if (id == null)
            {
                throw new ArgumentException("ID Khách hàng không được để trống");
            }

            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }

            var result = await _appDbContext.Activities
            .Where(c => c.CustomerId == id && partner.Id == partner.Id)
            .ToListAsync();

            if (result == null)
            {
                return new List<Activity>();
            }
            return result;
        }

        public async Task<List<Customer?>> GetAllAsync(Employee employee, Partner partner)
        {
            var result = await _appDbContext.Customers
   .Where(c => c.PartnerId == partner.Id)
   .ToListAsync();
            if (!IsOwner)
            {

                var employeeData = await _employeeService.FindByIdAsync(employee.Id);
                if (employeeData == null)
                {
                    throw new ArgumentException($"Employee with ID {employee.Id} does not exist.");
                }
                result = await _appDbContext.Customers
          .Include(c => c.CustomerEmployees)
          .Where(c => c.PartnerId == partner.Id
                      && c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id))
          .ToListAsync();

                return result.Any() ? result : new List<Customer>();
            }
            return result.Any() ? result : new List<Customer>();
        }

        public async Task<List<ContactDTO>> GetAllContactAvailableByCustomer(int id, Partner partner)
        {
            if (id == null)
            {
                throw new ArgumentException("ID khách hàng không được để trống !");
            }
            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }

            var result = await _appDbContext.Contacts
         .Where(c => !c.CustomerContacts.Any(cc => cc.CustomerId == id && cc.PartnerId == partner.Id))
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
         }).ToListAsync();
            return result;
        }

        public async Task<List<ContactDTO>> GetAllContactsByIdAsync(int id, Partner partner)
        {
            if (id == null)
            {
                throw new ArgumentException("ID khách hàng không được để trống !");
            }
            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }

            var result = await _appDbContext.Contacts
            .Where(c => c.CustomerContacts.Any(cc => cc.CustomerId == id && cc.PartnerId == partner.Id))
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
            }).ToListAsync();
            return result;
        }

        public async Task<List<Invoice>> GetAllInvoicesByIdAsync(int id, Partner partner)
        {
            if (id == null)
            {
                throw new ArgumentException("ID khách hàng không được để trống !");
            }
            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }

            var result = await _appDbContext.Invoices
          .Where(o => o.CustomerId == id && o.Partner.Id == partner.Id)
          .ToListAsync();


            if (result == null) return new List<Invoice>();
            return result;
        }

        public async Task<List<OptionalOrderDTO>> GetAllOrdersByIdAsync(int id, Partner partner)
        {
            if (id == null)
            {
                throw new ArgumentException("ID khách hàng không được để trống !");
            }
            if (partner == null)
            {
                throw new ArgumentException("Thông tin tổ chức không được bỏ trống");
            }

            var orders = await _appDbContext.Orders
            .Where(o => o.CustomerId == id && o.Partner.Id == partner.Id)
            .ToListAsync();

            if (!orders.Any())
            {
                return new List<OptionalOrderDTO>();
            }
            var orderDtos = orders.Select(order =>
            {
                var dto = _mapper.Map<OptionalOrderDTO>(order);
                return dto;
            }).ToList();

            return orderDtos;
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

        public async Task<GeneralResponse?> RemoveContactFromCustomer(int id, int contactId, Partner partner)
        {

            if (id == null)
            {
                return new GeneralResponse(false, "ID khách hàng không được để trống !");
            }
            if (contactId == null)
            {
                return new GeneralResponse(false, "Thông tin liên không được bỏ trống");
            }
            if (partner == null)
            {
                return new GeneralResponse(false, "Thông tin tổ chức không được bỏ trống");
            }
            var customerContact = await _appDbContext.CustomerContacts
         .FirstOrDefaultAsync(cc => cc.CustomerId == id
                                 && cc.ContactId == contactId
                                 && cc.PartnerId == partner.Id);
            if (customerContact == null)
            {
                return new GeneralResponse(false, "Không tìm thấy bản ghi cần xóa!");
            }
            _appDbContext.CustomerContacts.Remove(customerContact);

            await _appDbContext.SaveChangesAsync();


            return new GeneralResponse(true, "Xóa thành công!");

        }


        public async Task<GeneralResponse?> UpdateAsync(int id, CustomerDTO updateDto, Employee employee, Partner partner)
        {
            if (employee == null || partner == null)
            {
                return new GeneralResponse(false, "Không tìm thấy thông tin nhân viên hoặc tổ chức");
            }
            Console.WriteLine("Is checking customer existed in server");
            var existingCustomer = await _appDbContext.Customers
                            .Include(c => c.CustomerEmployees).AsNoTracking()
                            .FirstOrDefaultAsync(c =>
                                c.Id == id &&
                                c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.Employee.PartnerId == partner.Id));
            if (existingCustomer == null)
                return new GeneralResponse(false, "Không tìm thấy thông tin khách hàng để cập nhật");
            Console.WriteLine("Prepared to update existing customer");
            _mapper.Map(updateDto, existingCustomer);
            await _appDbContext.UpdateDb(existingCustomer);
            System.Console.WriteLine("Successfully updated customer");

            return new GeneralResponse(true, "Cập nhật thông tin khách hàng thành công.");


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
