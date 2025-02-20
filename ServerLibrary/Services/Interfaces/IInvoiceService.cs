using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<List<InvoiceDTO>> GetAllInvoicesAsync(Employee employee, Partner partner);

        Task<InvoiceDTO?> GetInvoiceByIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse?> CreateInvoiceAsync(InvoiceDTO invoiceDto, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateInvoiceAsync(int id, InvoiceDTO invoiceDTO, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, InvoiceDTO invoiceDTO, Employee employee, Partner partner);

        Task<GeneralResponse?> DeleteBulkInvoicesAsync(string ids, Employee employee, Partner partner);

        Task<GeneralResponse?> BulkUpdateInvoicesAsync(List<int> invoiceIds, int? ContactId, int? CustomerId,
         Employee employee, Partner partner);

        // Task<GeneralResponse?> RemoveOrderAsync(int orderId, int employeeId);
    }
}
