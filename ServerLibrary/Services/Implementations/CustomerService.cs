using System.Runtime.ConstrainedExecution;
using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Data.ThirdPartyModels;
using Mapper.CustomerMapper;
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

        private readonly IPartnerService _partnerService;
        // private readonly IImportLogger _importLogger;

        public CustomerService(AppDbContext appDbContext,
            IPartnerService partnerService,
              IMapper mapper,
             //    IImportLogger importLogger,
             IHttpContextAccessor httpContextAccessor
             ) : base(appDbContext, httpContextAccessor)
        {
            _appDbContext = appDbContext;
            _partnerService = partnerService;
            _mapper = mapper;
            // _importLogger = importLogger ?? throw new ArgumentNullException(nameof(importLogger));
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


            var checkCodeExisted = await CheckCustomerCodeAsync(customer.AccountNumber, employee, partner);
            if (checkCodeExisted != null && !checkCodeExisted.Flag)
                return new DataStringResponse(false, "Mã khách hàng đã tồn tại !");


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
                NumberOfDaysOwed = customer.NumberOfDaysOwed,
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

                CustomerSinceDate = customer.CustomerSinceDate,
                // ** Automatically assign to employee for Owner and Partner
                OwnerID = employee.Id.ToString(),
                OwnerIDName = employee.FullName,
                Partner = partner,
                Employee = employee,
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
                return new GeneralResponse(false, "Không tìm thấy khách hàng ");
            }
            var employeeExists = customer.CustomerEmployees
            .Any(ce => ce.EmployeeId == employee.Id && ce.AccessLevel == AccessLevel.ReadWrite);

            if (!employeeExists)
            {
                return new GeneralResponse(false, $"Employee with ID {employee.Id} does not have permission to remove this customer.");
            }

            _appDbContext.Customers.Remove(customer);

            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Xoá khách hàng thành công");
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

        public async Task<PagedResponse<List<ActivityDTO>>> GetAllActivitiesByIdAsync(int id, Partner partner, int pageNumber, int pageSize)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("ID khách hàng phải lớn hơn 0.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Thông tin tổ chức không được để trống.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                // Build query
                var query = _appDbContext.Activities
                    .Where(c => c.CustomerId == id && c.PartnerId == partner.Id)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                var activities = await query
                    .OrderBy(c => c.CreatedDate) // Sort for consistency
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var activityDtos = _mapper.Map<List<ActivityDTO>>(activities);

                return new PagedResponse<List<ActivityDTO>>(
                    data: activityDtos ?? new List<ActivityDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách hoạt động thất bại: {ex.Message}", ex);
            }
        }

        public async Task<PagedResponse<List<CustomerDTO>>> GetAllAsync(Employee employee,
         Partner partner, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20; // Default page size

            try
            {
                // Base query
                var query = _appDbContext.Customers
                    .AsNoTracking()
                    .Where(c => c.PartnerId == partner.Id);

                // Apply ownership filter
                // if (!IsOwner)
                // {
                //     // Kiểm tra employee tồn tại
                //     var employeeExists = await _appDbContext.Employees
                //         .AnyAsync(e => e.Id == employee.Id);
                //     if (!employeeExists)
                //     {
                //         throw new ArgumentException($"Employee với ID {employee.Id} không tồn tại.");
                //     }

                query = query
                    .Include(c => c.CustomerEmployees)
                    .Where(c => c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id));
                // }

                // Get total count before pagination
                int totalRecords = await query.CountAsync();

                // Return empty response if no data
                if (totalRecords == 0)
                {
                    return new PagedResponse<List<CustomerDTO>>(
                        data: new List<CustomerDTO>(),
                        pageNumber: pageNumber,
                        pageSize: pageSize,
                        totalRecords: 0
                    );
                }
                var pagedCustomers = await query
                    .OrderBy(c => c.Id) // Default sort, có thể thêm sortBy nếu cần
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var customerDtos = _mapper.Map<List<CustomerDTO>>(pagedCustomers);

                return new PagedResponse<List<CustomerDTO>>(
                    data: customerDtos.Select(customer =>
                    {
                        var dto = _mapper.Map<CustomerDTO>(customer);
                        return dto;
                    }).ToList(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi không xác định khi lấy danh sách khách hàng: {ex.Message}", ex);
            }
        }

        public async Task<PagedResponse<List<ContactDTO>>> GetAllContactAvailableByCustomer(int id, Partner partner, int pageNumber, int pageSize)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("ID khách hàng phải lớn hơn 0.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Thông tin tổ chức không được để trống.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10; // Default page size

                // Build query
                var query = _appDbContext.Contacts
                    .Where(c => !c.CustomerContacts.Any(cc => cc.CustomerId == id && cc.PartnerId == partner.Id))
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
                throw new Exception($"Lấy danh sách contacts thất bại: {ex.Message}", ex);
            }
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

        public async Task<PagedResponse<List<InvoiceDTO>>> GetAllInvoicesByIdAsync(int id, Partner partner, int pageNumber, int pageSize)
        {
            try
            {
                // Check inputs
                if (id <= 0)
                {
                    throw new ArgumentException("ID khách hàng phải lớn hơn 0.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Thông tin tổ chức không được để trống.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                // Build query
                var query = _appDbContext.Invoices
                    .Where(o => o.CustomerId == id && o.Partner.Id == partner.Id)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                var invoices = await query
                    .OrderBy(o => o.Id) // Sort for consistency
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var invoiceDtos = _mapper.Map<List<InvoiceDTO>>(invoices);

                return new PagedResponse<List<InvoiceDTO>>(
                    data: invoiceDtos ?? new List<InvoiceDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Lỗi tham số đầu vào: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách hóa đơn thất bại: {ex.Message}", ex);
            }
        }

        public async Task<PagedResponse<List<OptionalOrderDTO>>> GetAllOrdersByIdAsync(int id, Partner partner, int pageNumber, int pageSize)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("ID khách hàng phải lớn hơn 0.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Thông tin tổ chức không được để trống.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var query = _appDbContext.Orders
                    .Where(o => o.CustomerId == id && o.Partner.Id == partner.Id)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                var orders = await query
                    .OrderBy(o => o.Id) // Sort for consistency
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var orderDtos = _mapper.Map<List<OptionalOrderDTO>>(orders);

                return new PagedResponse<List<OptionalOrderDTO>>(
                    data: orderDtos ?? new List<OptionalOrderDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Lỗi tham số đầu vào: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách đơn hàng thất bại: {ex.Message}", ex);
            }
        }

        public async Task<OptionalCustomerDTO?> GetCustomerByIdAsync(int id, Employee employee, Partner partner)
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

            return customer.ToCustomerDTO();
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
            var codeGenerator = new GenerateNextCode(_appDbContext);

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
            if (string.IsNullOrEmpty(updateDto.AccountNumber))
            {
                updateDto.AccountNumber = await codeGenerator.GenerateNextCodeAsync<Customer>(
                    "KH",
                    c => c.AccountNumber,
                    c => c.PartnerId == partner.Id
                );
            }
            else
            {
                bool exists = await _appDbContext.Customers.AnyAsync(c =>
                    c.AccountNumber == updateDto.AccountNumber &&
                    c.PartnerId == partner.Id &&
                    c.Id != id);

                if (exists)
                {
                    var newAccountNumber = await codeGenerator.GenerateNextCodeAsync<Customer>(
                        "KH",
                        c => c.AccountNumber,
                        c => c.PartnerId == partner.Id
                    );
                    Console.WriteLine($"AccountNumber '{updateDto.AccountNumber}' already existed. Replaced with '{newAccountNumber}' for AccountNumber ID {id}.");
                    updateDto.AccountNumber = newAccountNumber;
                }
            }

            _mapper.Map(updateDto, existingCustomer);
            await _appDbContext.UpdateDb(existingCustomer);
            System.Console.WriteLine("Successfully updated customer");

            return new GeneralResponse(true, "Cập nhật thông tin khách hàng thành công.");
        }

        public async Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateCustomerDTO updateCustomer, Employee employee, Partner partner)
        {
            var existingCustomer = await _appDbContext.Customers
                    .Include(c => c.CustomerEmployees)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id &&
                        c.CustomerEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.Employee.PartnerId == partner.Id));

            if (existingCustomer == null)
                return new GeneralResponse(false, "Không tìm thấy thông tin khách hàng");


            var properties = typeof(UpdateCustomerDTO).GetProperties();
            foreach (var prop in properties)
            {
                var newValue = prop.GetValue(updateCustomer);
                if (newValue != null && newValue.ToString() != "")
                {
                    var existingProp = typeof(Customer).GetProperty(prop.Name);
                    if (existingProp != null)
                    {
                        existingProp.SetValue(existingCustomer, newValue);
                        _appDbContext.Entry(existingCustomer).Property(existingProp.Name).IsModified = true;
                    }
                }
                await _appDbContext.SaveChangesAsync();
            }
            return new GeneralResponse(true, "Cập nhật thông tin khách hàng thành công");
        }

        public async Task<GeneralResponse?> UnassignActivityFromCustomer(int id, int activityId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");

                    if (id == null)
                    {
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với khách hàng nào.");
                    }

                    var activity = await _appDbContext.Activities
                        .FirstOrDefaultAsync(a => a.Id == activityId && a.CustomerId == id && a.PartnerId == partner.Id);

                    if (activity == null)
                    {
                        Console.WriteLine($"No Activity found for ID {activityId}.");
                        return new GeneralResponse(true, $"ID {activityId} không liên kết với hoạt động nào.");
                    }

                    activity.CustomerId = null;
                    _appDbContext.Activities.Update(activity);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Activity removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa hoạt động khỏi khách hàng ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa hoạt động khỏi khách hàng ID {id}: {ex.Message}");
                }
            });
        }

        public async Task<GeneralResponse?> UnassignOrderFromCustomer(int id, int orderId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");
                    var customer = await _appDbContext.Customers
                        .FirstOrDefaultAsync(c => c.Id == id && c.Partner.Id == partner.Id);

                    if (customer == null)
                    {
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với khách hàng nào.");
                    }

                    var order = await _appDbContext.Orders
                        .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == id && o.Partner.Id == partner.Id);

                    if (order == null)
                    {
                        Console.WriteLine($"No Order found for ID {orderId}.");
                        return new GeneralResponse(true, $"ID {orderId} không liên kết với đơn hàng nào.");
                    }
                    order.CustomerId = null;
                    _appDbContext.Orders.Update(order);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Order removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa đơn hàng khỏi khách hàng ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa đơn hàng khỏi khách hàng ID {id}: {ex.Message}");
                }
            });
        }

        public async Task<GeneralResponse?> UnassignInvoiceFromCustomer(int id, int invoiceId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");
                    var customer = await _appDbContext.Customers
                        .FirstOrDefaultAsync(c => c.Id == id && c.Partner.Id == partner.Id);

                    if (customer == null)
                    {
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với khách hàng nào.");
                    }

                    var invoice = await _appDbContext.Invoices
                        .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CustomerId == id && i.Partner.Id == partner.Id);

                    if (invoice == null)
                    {
                        Console.WriteLine($"No Invoice found for ID {invoiceId}.");
                        return new GeneralResponse(true, $"ID {invoiceId} không liên kết với hóa đơn nào.");
                    }
                    invoice.CustomerId = null;
                    _appDbContext.Invoices.Update(invoice);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Invoice removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa hóa đơn khỏi khách hàng ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa hóa đơn khỏi khách hàng ID {id}: {ex.Message}");
                }
            });
        }

        private async Task<CustomerDTO> GetCustomerByCode(string code, Partner partner)
        {
            var existingCustomer = await _appDbContext.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == code && c.PartnerId == partner.Id);
            if (existingCustomer == null)
                return null;

            return new CustomerDTO
            {
                Id = existingCustomer.Id,
                AccountNumber = existingCustomer.AccountNumber,
                AccountName = existingCustomer.AccountName,
            };
        }
        public async Task<DataObjectResponse?> GenerateCustomerCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
                new DataStringResponse(false, "Thông tin tổ chức không để trống !", null);


            var customerCode = await codeGenerator
            .GenerateNextCodeAsync<Customer>(prefix: "KH",
                codeSelector: c => c.AccountNumber,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã khách hàng thành công", customerCode);
        }

        public async Task<DataObjectResponse?> CheckCustomerCodeAsync(string code, Employee employee, Partner partner)
        {
            var customerDetail = await GetCustomerByCode(code, partner);

            if (customerDetail == null)
            {
                return new DataObjectResponse(true, "Mã khách hàng có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã khách hàng đã tồn tại", new
                {
                    customerDetail.AccountNumber,
                    customerDetail.AccountName,
                    customerDetail.Id
                });
            }
        }


        public async Task<GeneralResponse?> UnassignTicketFromCustomer(int id, int ticketId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");
                    var customer = await _appDbContext.Customers
                        .FirstOrDefaultAsync(c => c.Id == id && c.Partner.Id == partner.Id);

                    if (customer == null)
                    {
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với khách hàng nào.");
                    }

                    var ticket = await _appDbContext.SupportTickets
                        .FirstOrDefaultAsync(o => o.Id == ticketId && o.CustomerId == id && o.Partner.Id == partner.Id);

                    if (ticket == null)
                    {
                        Console.WriteLine($"No Ticket found for ID {ticketId}.");
                        return new GeneralResponse(true, $"ID {ticketId} không liên kết với khách hàng nào.");
                    }
                    ticket.CustomerId = null;
                    _appDbContext.SupportTickets.Update(ticket);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Ticket removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa thẻ tư vấn khỏi khách hàng ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa thẻ tư vấn khỏi khách hàng ID {id}: {ex.Message}");
                }
            });
        }

        public async Task<GeneralResponse?> UnassignQuoteFromCustomer(int id, int quoteId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");
                    var customer = await _appDbContext.Customers
                        .FirstOrDefaultAsync(c => c.Id == id && c.Partner.Id == partner.Id);

                    if (customer == null)
                    {
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với khách hàng nào.");
                    }

                    var quote = await _appDbContext.Quotes
                        .FirstOrDefaultAsync(o => o.Id == quoteId && o.CustomerId == id && o.Partner.Id == partner.Id);

                    if (quote == null)
                    {
                        Console.WriteLine($"No Quote found for ID {quoteId}.");
                        return new GeneralResponse(true, $"ID {quoteId} không liên kết với khách hàng nào.");
                    }
                    quote.CustomerId = null;
                    _appDbContext.Quotes.Update(quote);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Quote removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa báo giá khỏi khách hàng ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa báo giá khỏi khách hàng ID {id}: {ex.Message}");
                }
            });
        }

        public async Task<GeneralResponse?> UnassignCustomerCareTicketFromCustomer(int id, int customerCareTicketId, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Execution strategy started.");
                    var customer = await _appDbContext.Customers
                        .FirstOrDefaultAsync(c => c.Id == id && c.Partner.Id == partner.Id);

                    if (customer == null)
                    {
                        Console.WriteLine($"No Customer found for ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với khách hàng nào.");
                    }

                    var customerCareTicket = await _appDbContext.CustomerCares
                        .FirstOrDefaultAsync(o => o.Id == customerCareTicketId && o.CustomerId == id && o.Partner.Id == partner.Id);

                    if (customerCareTicket == null)
                    {
                        Console.WriteLine($"No Customer Care Ticket found for ID {customerCareTicketId}.");
                        return new GeneralResponse(true, $"ID {customerCareTicketId} không liên kết với khách hàng nào.");
                    }
                    customerCareTicket.CustomerId = null;
                    _appDbContext.CustomerCares.Update(customerCareTicket);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Customer Care Ticket removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa thẻ chăm sóc khỏi khách hàng ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Lỗi khi xóa thẻ chăm sóc khỏi khách hàng ID {id}: {ex.Message}");
                }
            });
        }

        public async Task<PagedResponse<List<QuoteDTO>>> GetAllQuotesByIdAsync(int id, Partner partner, int pageNumber, int pageSize)
        {
            try
            {
                // Check inputs
                if (id <= 0)
                {
                    throw new ArgumentException("ID khách hàng phải lớn hơn 0.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Thông tin tổ chức không được để trống.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                // Build query
                var query = _appDbContext.Quotes
                    .Where(o => o.CustomerId == id && o.Partner.Id == partner.Id)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                var invoices = await query
                    .OrderBy(o => o.Id) // Sort for consistency
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var quoteDtos = _mapper.Map<List<QuoteDTO>>(invoices);

                return new PagedResponse<List<QuoteDTO>>(
                    data: quoteDtos ?? new List<QuoteDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Lỗi tham số đầu vào: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách báo giá thất bại: {ex.Message}", ex);
            }
        }

        public async Task<PagedResponse<List<SupportTicketDTO>>> GetAllTicketsByIdAsync(int id, Partner partner, int pageNumber, int pageSize)
        {
            try
            {
                // Check inputs
                if (id <= 0)
                {
                    throw new ArgumentException("ID khách hàng phải lớn hơn 0.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Thông tin tổ chức không được để trống.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                // Build query
                var query = _appDbContext.SupportTickets
                    .Where(o => o.CustomerId == id && o.Partner.Id == partner.Id)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                var invoices = await query
                    .OrderBy(o => o.Id) // Sort for consistency
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var ticketDtos = _mapper.Map<List<SupportTicketDTO>>(invoices);

                return new PagedResponse<List<SupportTicketDTO>>(
                    data: ticketDtos ?? new List<SupportTicketDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Lỗi tham số đầu vào: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách thẻ tư vấn thất bại: {ex.Message}", ex);
            }
        }

        public async Task<PagedResponse<List<CustomerCareTicketDTO>>> GetAllCustomerCaresByIdAsync(int id, Partner partner, int pageNumber, int pageSize)
        {
            try
            {
                // Check inputs
                if (id <= 0)
                {
                    throw new ArgumentException("ID khách hàng phải lớn hơn 0.");
                }
                if (partner == null)
                {
                    throw new ArgumentNullException(nameof(partner), "Thông tin tổ chức không được để trống.");
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                // Build query
                var query = _appDbContext.CustomerCares
                    .Where(o => o.CustomerId == id && o.Partner.Id == partner.Id)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                var invoices = await query
                    .OrderBy(o => o.Id) // Sort for consistency
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var customerCareTicketDtos = _mapper.Map<List<CustomerCareTicketDTO>>(invoices);

                return new PagedResponse<List<CustomerCareTicketDTO>>(
                    data: customerCareTicketDtos ?? new List<CustomerCareTicketDTO>(),
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    totalRecords: totalRecords
                );
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Lỗi tham số đầu vào: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lấy danh sách thẻ chăm sóc thất bại: {ex.Message}", ex);
            }
        }

        public async Task<ImportResultDto<CustomerDTO>> ImportCustomerDataAsync(
    List<CustomerDTO> data,
    Employee employee,
    Partner partner)
        {
            var result = new ImportResultDto<CustomerDTO>();
            int rowIndex = 0;
            foreach (var record in data)
            {
                rowIndex++;
                try
                {
                    if (string.IsNullOrEmpty(record.AccountNumber))
                    {
                        result.Errors.Add(new ImportError<CustomerDTO>
                        {
                            Row = rowIndex,
                            Flag = true,
                            Data = new CustomerDTO
                            {
                                AccountNumber = record.AccountNumber,
                                AccountName = record.AccountName,
                            },
                            Message = "Mã khách hàng (AccountNumber) không được để trống"
                        });
                        continue;
                    }

                    var existingCustomer = await _appDbContext.Customers
                        .FirstOrDefaultAsync(c => c.AccountNumber == record.AccountNumber && c.PartnerId == partner.Id);

                    if (existingCustomer != null)
                    {
                        try
                        {
                            _mapper.Map(record, existingCustomer);
                            _appDbContext.Update(existingCustomer);
                            await _appDbContext.SaveChangesAsync();
                            result.Updated.Add(record);
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new ImportError<CustomerDTO>
                            {
                                Row = rowIndex,
                                Flag = false,
                                Data = new CustomerDTO
                                {
                                    AccountNumber = record.AccountNumber,
                                    AccountName = record.AccountName,
                                },
                                Message = $"Lỗi khi cập nhật: {ex.Message}"
                            });
                        }
                    }
                    else
                    {
                        var createCustomerDto = MapToCreateCustomer(record, employee, partner);
                        var createResult = await CreateAsync(createCustomerDto, employee, partner);

                        if (createResult.Flag)
                        {
                            result.Added.Add(record);
                        }
                        else
                        {
                            result.Errors.Add(new ImportError<CustomerDTO>
                            {
                                Row = rowIndex,
                                Flag = false,
                                Data = new CustomerDTO
                                {
                                    AccountNumber = record.AccountNumber,
                                    AccountName = record.AccountName,
                                },
                                Message = createResult.Message ?? "Tạo khách hàng thất bại"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ImportError<CustomerDTO>
                    {
                        Row = rowIndex,
                        Flag = false,
                        Data = new CustomerDTO
                        {
                            AccountNumber = record.AccountNumber,
                            AccountName = record.AccountName,
                        },
                        Message = $"Lỗi hệ thống: {ex.Message}"
                    });
                }
            }
            return result;
        }

        private CreateCustomer MapToCreateCustomer(CustomerDTO record, Employee employee, Partner partner)
        {
            return new CreateCustomer
            {
                AccountNumber = record.AccountNumber,
                AccountName = record.AccountName ?? "",
                AccountReferredID = record.AccountReferredID,
                AccountShortName = record.AccountShortName,
                AccountTypeID = record.AccountTypeID,
                Avatar = record.Avatar,
                BankAccount = record.BankAccount,
                BankName = record.BankName,
                BillingCode = record.BillingCode,
                BillingCountryID = record.BillingCountryID ?? "",
                BillingDistrictID = record.BillingDistrictID ?? "",
                BillingProvinceID = record.BillingProvinceID ?? "",
                BillingStreet = record.BillingStreet,
                BillingWardID = record.BillingWardID,
                BillingLat = record.BillingLat,
                BillingLong = record.BillingLong,
                BudgetCode = record.BudgetCode,
                BusinessTypeID = record.BusinessTypeID,
                CelebrateDate = record.CelebrateDate,
                Debt = record.Debt,
                DebtLimit = record.DebtLimit,
                Description = record.Description,
                NumberOfDaysOwed = record.NumberOfDaysOwed,
                Fax = record.Fax,
                GenderID = record.GenderID,
                Identification = record.Identification,
                Inactive = record.Inactive,
                IndustryID = record.IndustryID,
                Latitude = record.Latitude,
                LeadSourceID = record.LeadSourceID,
                NoOfEmployeeID = record.NoOfEmployeeID,
                OfficeEmail = record.OfficeEmail ?? "",
                OfficeTel = record.OfficeTel ?? "",
                AnnualRevenueID = record.AnnualRevenueID,
                RevenueDetail = record.RevenueDetail,
                SectorText = record.SectorText,
                ShippingCode = record.ShippingCode,
                ShippingCountryID = record.ShippingCountryID,
                ShippingDistrictID = record.ShippingDistrictID,
                ShippingLat = record.ShippingLat,
                ShippingLong = record.ShippingLong,
                ShippingProvinceID = record.ShippingProvinceID,
                ShippingStreet = record.ShippingStreet,
                ShippingWardID = record.ShippingWardID,
                TaxCode = record.TaxCode,
                CustomerSinceDate = record.CustomerSinceDate ?? DateTime.Now,
                Website = record.Website,
                IsPublic = record.IsPublic,
                IsPartner = record.IsPartner,
                IsPersonal = record.IsPersonal,
                IsOldCustomer = record.IsOldCustomer,
                IsDistributor = record.IsDistributor,
                OwnerID = employee.Id.ToString(),
                OwnerIDName = employee.FullName,
                PartnerId = partner.Id,
                EmployeeId = employee.Id,
            };
        }

    }
}
