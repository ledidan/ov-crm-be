using System.Security.Claims;
using AutoMapper;
using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Mapper.ContactMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ContactService : BaseService, IContactService
    {
        private readonly AppDbContext _appDbContext;

        private readonly IMapper _mapper;
        private readonly IPartnerService _partnerService;
        private readonly IEmployeeService _employeeService;
        public ContactService(AppDbContext appDbContext,
        IPartnerService partnerService, IEmployeeService employeeService,
        IMapper mapper, IHttpContextAccessor httpContextAccessor
        ) : base(appDbContext, httpContextAccessor)
        {
            _appDbContext = appDbContext;
            _partnerService = partnerService;
            _employeeService = employeeService;
            _mapper = mapper;
        }

        public async Task<DataObjectResponse> CreateAsync(CreateContact contact, Employee employee, Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);
            if (contact == null)
                return new DataObjectResponse(false, "Thông tin liên hệ rỗng !");


            var checkCodeExisted = await CheckContactCodeAsync(contact.ContactCode, employee, partner);
            if (checkCodeExisted != null && !checkCodeExisted.Flag)
                return new DataObjectResponse(false, "Mã liên hệ đã tồn tại !");

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
                return new DataObjectResponse(false, "Thông tin tổ chức không để trống !");
            if (employee != null)
            {
                var employeeData = await _employeeService.FindByIdAsync(employee.Id);
                if (employeeData == null)
                    return new DataObjectResponse(false, "Không tìm thấy nhân viên");
            }
            else
            {
                return new DataObjectResponse(false, "ID Nhân viên không có !");
            }
            if (string.IsNullOrEmpty(contact.ContactCode))
            {
                contact.ContactCode = await codeGenerator.GenerateNextCodeAsync<Contact>(
            "LH",
            c => c.ContactCode,
            c => c.PartnerId == partner.Id
        );
            }

            var newContact = new Contact()
            {
                ContactCode = contact.ContactCode,
                ContactName = contact.ContactName,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                Email = contact.Email,
                OfficeTel = contact.OfficeTel,
                DateOfBirth = contact.DateOfBirth,
                Description = contact.Description,
                AccountTypeID = contact.AccountTypeID,
                DepartmentID = contact.DepartmentID,
                LeadSourceID = contact.LeadSourceID,
                MailingDistrictID = contact.MailingDistrictID,
                MailingProvinceID = contact.MailingProvinceID,
                MailingStreet = contact.MailingStreet,
                MailingWardID = contact.MailingWardID,
                MailingZip = contact.MailingZip,
                Mobile = contact.Mobile,
                OfficeEmail = contact.OfficeEmail,
                OtherPhone = contact.OtherPhone,
                SalutationID = contact.SalutationID,
                ShippingDistrictID = contact.ShippingDistrictID,
                ShippingProvinceID = contact.ShippingProvinceID,
                ShippingStreet = contact.ShippingStreet,
                ShippingWardID = contact.ShippingWardID,
                ShippingZip = contact.ShippingZip,
                TitleID = contact.TitleID,
                Zalo = contact.Zalo,
                IsPublic = contact.IsPublic,
                EmailOptOut = contact.EmailOptOut,
                PhoneOptOut = contact.PhoneOptOut,
                CustomerId = contact.CustomerId,
                CustomerName = contact.CustomerName,
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                // ** Automatically assign to employee for Owner and Partner
                OwnerID = employee.Id,
                OwnerIDName = employee.FullName,
                PartnerId = partner.Id,
                PartnerName = partner.Name,
                ContactEmployees = new List<ContactEmployees>
        {
            new ContactEmployees
            {
                Employee = employee,
                EmployeeId = employee.Id,
                Partner = partner,
                PartnerId = partner.Id,
                AccessLevel = AccessLevel.ReadWrite,
            }
        }
            };

            await _appDbContext.InsertIntoDb(newContact);

            return new DataObjectResponse(true, "Tạo liên hệ thành công !", newContact.Id.ToString());
        }

        public Task<GeneralResponse?> DeleteAsync(int id, int employeeId)
        {
            throw new NotImplementedException();
        }

        public async Task<Contact?> GetByIdAsync(int id, Employee employee, Partner partner)
        {
            var contact = await _appDbContext.Contacts
        .Include(c => c.ContactEmployees)
        .FirstOrDefaultAsync(c =>
            c.Id == id &&
            c.ContactEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.Employee.PartnerId == partner.Id));
            return contact;
        }

        public async Task<GeneralResponse?> UpdateContactIdAsync(int id, UpdateContactDTO updateContact, Employee employee, Partner partner)
        {

            var codeGenerator = new GenerateNextCode(_appDbContext);
            var existingContact = await _appDbContext.Contacts
        .FirstOrDefaultAsync(c => c.Id == id && c.EmployeeId == employee.Id && c.PartnerId == partner.Id);

            var updatedContact = updateContact.FromUpdateContactDTO();

            if (existingContact == null)
                return new GeneralResponse(false, "Không tìm thấy ID liên hệ để cập nhất");

            if (string.IsNullOrEmpty(updateContact.ContactCode))
            {
                string originalCode = updateContact.ContactCode;
                bool exists = await _appDbContext.Contacts.AnyAsync(c =>
     c.ContactCode == updateContact.ContactCode &&
     c.PartnerId == partner.Id &&
     c.Id != id);
                if (exists)
                {
                    updateContact.ContactCode = await codeGenerator.GenerateNextCodeAsync<Contact>("LH", c => c.ContactCode, c => c.PartnerId == partner.Id);
                    Console.WriteLine($"ContactCode '{originalCode}' already existed. Replaced with '{updateContact.ContactCode}' for Contact ID {id}.");
                }
            }
            existingContact.AccountTypeID = updateContact.AccountTypeID;
            existingContact.ContactCode = updateContact.ContactCode;
            existingContact.ContactName = updateContact.ContactName;
            existingContact.DepartmentID = updateContact.DepartmentID;
            existingContact.LeadSourceID = updateContact.LeadSourceID;
            existingContact.FirstName = updateContact.FirstName;
            existingContact.LastName = updateContact.LastName;
            existingContact.Email = updateContact.Email;
            existingContact.OfficeTel = updateContact.OfficeTel;
            existingContact.DateOfBirth = updateContact.DateOfBirth;
            existingContact.Description = updateContact.Description;
            existingContact.MailingDistrictID = updateContact.MailingDistrictID;
            existingContact.MailingProvinceID = updateContact.MailingProvinceID;
            existingContact.MailingStreet = updateContact.MailingStreet;
            existingContact.MailingWardID = updateContact.MailingWardID;
            existingContact.MailingZip = updateContact.MailingZip;
            existingContact.Mobile = updateContact.Mobile;
            existingContact.OfficeEmail = updateContact.OfficeEmail;
            existingContact.OtherPhone = updateContact.OtherPhone;
            existingContact.SalutationID = updateContact.SalutationID;
            existingContact.ShippingDistrictID = updateContact.ShippingDistrictID;
            existingContact.ShippingProvinceID = updateContact.ShippingProvinceID;
            existingContact.ShippingStreet = updateContact.ShippingStreet;
            existingContact.ShippingWardID = updateContact.ShippingWardID;
            existingContact.ShippingZip = updateContact.ShippingZip;
            existingContact.TitleID = updateContact.TitleID;
            existingContact.Zalo = updateContact.Zalo;
            existingContact.IsPublic = updateContact.IsPublic;
            existingContact.EmailOptOut = updateContact.EmailOptOut;
            existingContact.PhoneOptOut = updateContact.PhoneOptOut;

            await _appDbContext.UpdateDb(existingContact);
            return new GeneralResponse(true, "Cập nhật liên hệ thành công.");
        }


        public async Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateContactDTO updateContact,
         Employee employee, Partner partner)
        {
            var existingContact = await _appDbContext.Contacts.FirstOrDefaultAsync(
                c => c.Id == id && c.PartnerId == partner.Id
            );

            if (existingContact == null)
            {
                return new GeneralResponse(false, "Không tìm thấy ID liên hệ để cập nhất");
            }

            var properties = typeof(UpdateContactDTO).GetProperties();

            foreach (var prop in properties)
            {
                var newValue = prop.GetValue(updateContact);
                if (newValue != null && newValue.ToString() != "")
                {
                    var existingProp = typeof(Contact).GetProperty(prop.Name);
                    if (existingProp != null)
                    {
                        existingProp.SetValue(existingContact, newValue);
                        _appDbContext.Entry(existingContact).Property(existingProp.Name).IsModified = true;
                    }
                }
                await _appDbContext.SaveChangesAsync();

            }
            return new GeneralResponse(true, "Cập nhật liên hệ thành công");
        }

        public async Task<GeneralResponse?> DeleteIdAsync(int id, Employee employee, Partner partner)
        {
            var existingContact = await _appDbContext.Contacts
       .Include(c => c.ContactEmployees)
       .Where(s => s.PartnerId == partner.Id)
       .FirstOrDefaultAsync(c => c.Id == id);

            if (existingContact == null)
            {
                return new GeneralResponse(false, "Không tìm thấy liên hệ");
            }

            var creatorEmployee = existingContact.ContactEmployees
            .FirstOrDefault(ce => ce.EmployeeId == employee.Id && ce.AccessLevel == AccessLevel.ReadWrite);

            if (creatorEmployee == null)
            {
                return new GeneralResponse(false, "Bạn không có quyền xoá liên hệ");
            }

            _appDbContext.Contacts.Remove(existingContact);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Xoá liên hệ thành công");
        }

        public async Task<List<Contact>> GetAllAsync(Employee employee, Partner partner)
        {
            var result = await _appDbContext.Contacts
    .Where(c => c.PartnerId == partner.Id)
    .ToListAsync();

            if (!IsOwner)
            {
                var employeeData = await _employeeService.FindByIdAsync(employee.Id);
                if (employeeData == null)
                {
                    throw new ArgumentException($"Employee with ID {employee.Id} does not exist.");
                }
                result = await _appDbContext.Contacts
                    .Include(c => c.ContactEmployees)
                    .Where(c => c.PartnerId == partner.Id
                                && c.ContactEmployees.Any(ce => ce.EmployeeId == employee.Id))
                    .ToListAsync();
            }
            return result.Any() ? result : new List<Contact>();
        }

        public async Task<GeneralResponse?> DeleteBulkContacts(string ids, Employee employee, Partner partner)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return new GeneralResponse(false, "No contacts provided for deletion");
            }

            // Convert comma-separated string into a List<int>
            var idList = ids.Split(',')
                            .Select(id => int.TryParse(id, out int parsedId) ? parsedId : (int?)null)
                            .Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .ToList();

            if (!idList.Any())
            {
                return new GeneralResponse(false, "Invalid contact IDs provided");
            }

            var contacts = await _appDbContext.Contacts
                .Include(c => c.ContactEmployees)
                .Where(c => idList.Contains(c.Id) && c.PartnerId == partner.Id)
                .ToListAsync();

            if (!contacts.Any())
            {
                return new GeneralResponse(false, "Contacts not found");
            }

            var unauthorizedContacts = contacts
                .Where(c => !c.ContactEmployees.Any(ce => ce.EmployeeId == employee.Id && ce.AccessLevel == AccessLevel.ReadWrite))
                .ToList();

            if (unauthorizedContacts.Any())
            {
                return new GeneralResponse(false, "You are not authorized to delete some contacts");
            }

            _appDbContext.Contacts.RemoveRange(contacts);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Contacts deleted successfully");
        }


        public async Task<List<OptionalOrderDTO?>> GetAllOrdersByContactAsync(int contactId, Employee employee, Partner partner)
        {
            try
            {
                if (contactId == null)
                {
                    throw new ArgumentNullException(nameof(contactId), "Contact cannot be null.");
                }
                var orders = await _appDbContext.Orders
                      .Where(o =>
                          o.Partner == partner && o.ContactId == contactId && o.OwnerId == employee.Id ||
                          o.OrderEmployees.Any(oe => oe.EmployeeId == employee.Id))
                          .Include(oce => oce.OrderEmployees)
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
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve orders: {ex.Message}");
            }
        }

        public async Task<List<ContactInvoiceDTO?>> GetAllInvoicesByContactAsync(int contactId, Employee employee, Partner partner)
        {
            try
            {
                if (contactId == null)
                {
                    throw new ArgumentNullException(nameof(contactId), "ID Liên hệ không được để trống.");
                }
                var invoices = await _appDbContext.Invoices
                      .Where(o =>
                          o.Partner == partner && o.BuyerId == contactId && o.OwnerId == employee.Id ||
                          o.InvoiceEmployees.Any(oe => oe.EmployeeId == employee.Id))
                          .Include(oce => oce.InvoiceEmployees)
                      .ToListAsync();

                if (!invoices.Any())
                {
                    return new List<ContactInvoiceDTO?>();
                }
                var invoiceDtos = invoices.Select(invoice =>
                {
                    var dto = _mapper.Map<ContactInvoiceDTO>(invoice);
                    return dto;
                }).ToList();

                return invoiceDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve invoices: {ex.Message}");
            }
        }

        public async Task<GeneralResponse?> AssignContactToOrderAsync(int id, AssignOrderRequest request, Employee employee, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Execution strategy started.");
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    if (request.OrderIds == null || !request.OrderIds.Any())
                    {
                        return new GeneralResponse(false, "Danh sách đơn hàng rỗng.");
                    }

                    Console.WriteLine($"Fetching Orders for Order IDs: {string.Join(", ", request.OrderIds)}...");
                    var orders = await _appDbContext.Orders
                        .Where(o => request.OrderIds.Contains(o.Id) && o.Partner.Id == partner.Id)
                        .ToListAsync();

                    if (!orders.Any())
                    {
                        return new GeneralResponse(false, "Không tìm thấy đơn hàng nào phù hợp.");
                    }

                    foreach (var order in orders)
                    {
                        order.ContactId = id;
                    }

                    _appDbContext.Orders.UpdateRange(orders);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine($"Assigned Contact ID {id} to {orders.Count} orders.");

                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã liên kết liên hệ ID {id} với {orders.Count} đơn hàng.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred: {ex.Message}");
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction rolled back.");
                    return new GeneralResponse(false, $"Không thể liên kết đơn hàng với liên hệ: {ex.Message}");
                }
            });
        }
        public async Task<GeneralResponse?> UnassignContactToOrderAsync(int id, AssignOrderRequest request, Employee employee, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Execution strategy started.");
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    if (request.OrderIds == null || !request.OrderIds.Any())
                    {
                        return new GeneralResponse(false, "Danh sách đơn hàng rỗng.");
                    }

                    Console.WriteLine($"Fetching Orders for Order IDs: {string.Join(", ", request.OrderIds)}...");
                    var orders = await _appDbContext.Orders
                        .Where(o => request.OrderIds.Contains(o.Id) && o.Partner.Id == partner.Id)
                        .ToListAsync();

                    if (!orders.Any())
                    {
                        return new GeneralResponse(false, "Không tìm thấy đơn hàng nào phù hợp.");
                    }

                    foreach (var order in orders)
                    {
                        order.ContactId = null;
                    }

                    _appDbContext.Orders.UpdateRange(orders);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine($"Unassigned Contact ID {id} from {orders.Count} orders.");
                    await transaction.CommitAsync();
                    return new GeneralResponse(true, $"Đã hủy liên kết liên hệ ID {id} khỏi {orders.Count} đơn hàng.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    return new GeneralResponse(false, $"Lỗi khi hủy liên kết: {ex.Message}");
                }
            });
        }

        public async Task<GeneralResponse?> UnassignInvoiceFromContactAsync(int id, int invoiceId, Employee employee, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Execution strategy started.");
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    Console.WriteLine($"Fetching Invoices for Contact ID {id}...");

                    var invoice = await _appDbContext.Invoices
                        .FirstOrDefaultAsync(a => a.Id == invoiceId
                         && a.BuyerId == id && a.Partner.Id == partner.Id);

                    if (invoice == null)
                    {
                        Console.WriteLine($"No Invoices found for Contact ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với hóa đơn nào.");
                    }
                    invoice.BuyerId = null;
                    _appDbContext.Invoices.Update(invoice);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Invoices removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa liên kết hóa đơn khỏi liên hệ ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    return new GeneralResponse(false, $"Lỗi khi xóa liên kết hóa đơn: {ex.Message}");
                }
            });
        }

        public async Task<List<ActivityDTO?>> GetAllActivitiesByContactAsync(int contactId, Employee employee, Partner partner)
        {
            var activities = await _appDbContext.Activities
                .Where(a => a.ContactId == contactId && a.PartnerId == partner.Id)
                .ToListAsync();
            var activityDtos = activities.Select(activity =>
            {
                var dto = _mapper.Map<ActivityDTO>(activity);
                return dto;
            }).ToList();

            return activityDtos;
        }

        public async Task<GeneralResponse?> UnassignActivityFromContactAsync(int id, int activityId, Employee employee, Partner partner)
        {
            var strategy = _appDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                Console.WriteLine("Execution strategy started.");
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Transaction started.");

                    Console.WriteLine($"Fetching Activities for Contact ID {id}...");

                    var activity = await _appDbContext.Activities
                        .FirstOrDefaultAsync(a => a.Id == activityId
                         && a.ContactId == id && a.PartnerId == partner.Id);

                    if (activity == null)
                    {
                        Console.WriteLine($"No Activities found for Contact ID {id}.");
                        return new GeneralResponse(true, $"ID {id} không liên kết với hoạt động nào.");
                    }
                    activity.ContactId = null;
                    _appDbContext.Activities.Update(activity);
                    await _appDbContext.SaveChangesAsync();

                    Console.WriteLine("Activities removed successfully.");
                    await transaction.CommitAsync();
                    Console.WriteLine("Transaction committed successfully.");
                    return new GeneralResponse(true, $"Đã xóa liên kết hoạt động khỏi liên hệ ID {id}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    await transaction.RollbackAsync();
                    return new GeneralResponse(false, $"Lỗi khi xóa liên kết hoạt động: {ex.Message}");
                }
            });
        }
        private async Task<ContactDTO> GetContactByCode(string code, Partner partner)
        {
            var existingContact = await _appDbContext.Contacts
                .FirstOrDefaultAsync(c => c.ContactCode == code && c.PartnerId == partner.Id);
            if (existingContact == null)
                return null;

            return new ContactDTO
            {
                Id = existingContact.Id,
                ContactCode = existingContact.ContactCode,
                ContactName = existingContact.ContactName,
            };
        }

        public async Task<DataObjectResponse?> CheckContactCodeAsync(string code, Employee employee, Partner partner)
        {
            var contactDetail = await GetContactByCode(code, partner);

            if (contactDetail == null)
            {
                return new DataObjectResponse(true, "Mã liên hệ có thể sử dụng", null);
            }
            else
            {
                return new DataObjectResponse(false, "Mã liên hệ đã tồn tại", new
                {
                    contactDetail.ContactCode,
                    contactDetail.ContactName,
                    contactDetail.Id
                });
            }
        }

        public async Task<DataObjectResponse?> GenerateContactCodeAsync(Partner partner)
        {
            var codeGenerator = new GenerateNextCode(_appDbContext);

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
                new DataStringResponse(false, "Thông tin tổ chức không để trống !", null);


            var contactCode = await codeGenerator
            .GenerateNextCodeAsync<Contact>(prefix: "LH",
                codeSelector: c => c.ContactCode,
                filter: c => c.PartnerId == partner.Id);

            return new DataObjectResponse(true, "Tạo mã liên hệ thành công", contactCode);
        }
    }
}
