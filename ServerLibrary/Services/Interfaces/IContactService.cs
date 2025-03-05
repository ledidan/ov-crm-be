using System.Security.Claims;
using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IContactService
    {
        Task<GeneralResponse> CreateAsync(CreateContact contact, Employee employee, Partner partner);

        Task<Contact?> GetByIdAsync(int id, Employee employee, Partner partner);

        Task<List<Contact>> GetAllAsync(Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateContactIdAsync(int id, UpdateContactDTO updateContact, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateContactDTO updateContactDTO, Employee employee, Partner partner);

        Task<GeneralResponse?> DeleteBulkContacts(string ids, Employee employee, Partner partner);

        Task<GeneralResponse?> DeleteIdAsync(int id, Employee employee, Partner partner);

        Task<List<ContactOrderDTO?>> GetAllOrdersByContactAsync(int contactId, Employee employee, Partner partner);

        Task<List<ContactInvoiceDTO?>> GetAllInvoicesByContactAsync(int contactId, Employee employee, Partner partner);
    }
}