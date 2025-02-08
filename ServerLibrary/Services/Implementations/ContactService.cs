using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Mapper.ContactMapper;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ContactService : IContactService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPartnerService _partnerService;
        private readonly IEmployeeService _employeeService;

        public ContactService(AppDbContext appDbContext,
        IPartnerService partnerService, IEmployeeService employeeService)
        {
            _appDbContext = appDbContext;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }
        public async Task<GeneralResponse> CreateAsync(CreateContact contact, Employee employee, Partner partner)
        {
            if (contact == null)
                return new GeneralResponse(false, "Model is empty");

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
                return new GeneralResponse(false, "Partner not found");
            if (employee != null)
            {
                var employeeData = await _employeeService.FindByIdAsync(employee.Id);
                if (employeeData == null)
                    return new GeneralResponse(false, "Employee not found");
            }
            else
            {
                return new GeneralResponse(false, "EmployeeId is required");
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
                Partner = partner,
                PartnerId = partner.Id,
                EmployeeId = employee.Id,
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

            return new GeneralResponse(true, "Contact created successfully");
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
            var existingContact = await _appDbContext.Contacts
        .FirstOrDefaultAsync(c => c.Id == id && c.EmployeeId == employee.Id && c.PartnerId == partner.Id);
            var updatedContact = updateContact.FromUpdateContactDTO();
            if (existingContact == null)
                return new GeneralResponse(false, "Contact not found");
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
            // var existingEmployees = existingContact.ContactEmployees.ToDictionary(ce => ce.EmployeeId);
            // foreach (var contactEmployee in existingContact.ContactEmployees)
            // {
            //     if (contactEmployee != null && contactEmployee.EmployeeId == newEmployeeId)
            //     {
            //         contactEmployee.AccessLevel = accessLevel;
            //     }
            // }
            // if (!existingContact.ContactEmployees.Any(ce => ce.EmployeeId == newEmployeeId))
            // {
            //     if (existingContact?.PartnerId == null)
            //         return new GeneralResponse(false, "Partner information is missing for this contact");
            //     existingContact.ContactEmployees.Add(new ContactEmployees
            //     {
            //         ContactId = existingContact.Id,
            //         EmployeeId = newEmployeeId,
            //         PartnerId = existingContact.PartnerId,
            //         AccessLevel = accessLevel
            //     });
            // }

            await _appDbContext.UpdateDb(existingContact);
            return new GeneralResponse(true, "Contact updated successfully");
        }
        public async Task<GeneralResponse?> DeleteIdAsync(int id, Employee employee, Partner partner)
        {
            var existingContact = await _appDbContext.Contacts
       .Include(c => c.ContactEmployees)
       .Where(s => s.PartnerId == partner.Id)
       .FirstOrDefaultAsync(c => c.Id == id);

            if (existingContact == null)
            {
                return new GeneralResponse(false, "Contact not found");
            }

            var creatorEmployee = existingContact.ContactEmployees
            .FirstOrDefault(ce => ce.EmployeeId == employee.Id && ce.AccessLevel == AccessLevel.ReadWrite);

            if (creatorEmployee == null)
            {
                return new GeneralResponse(false, "You are not authorized to delete this contact");
            }

            _appDbContext.Contacts.Remove(existingContact);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Contact deleted successfully");
        }

        public async Task<List<Contact>> GetAllAsync(Employee employee, Partner partner)
        {
            var employeeData = await _employeeService.FindByIdAsync(employee.Id);
            if (employeeData == null)
            {
                throw new ArgumentException($"Employee with ID {employee.Id} does not exist.");
            }
            var result = await _appDbContext.Contacts
       .Include(c => c.ContactEmployees)
       .Where(c => c.PartnerId == partner.Id
                   && c.ContactEmployees.Any(ce => ce.EmployeeId == employee.Id))
       .ToListAsync();

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
    }
}