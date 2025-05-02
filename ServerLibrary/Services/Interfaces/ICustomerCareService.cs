



using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ICustomerCareService
    {

        Task<DataObjectResponse?> GenerateCustomerCareCodeAsync(Partner partner);
        Task<DataObjectResponse?> CheckCustomerCareCodeAsync(string code, Employee employee, Partner partner);
        Task<PagedResponse<List<CustomerCareTicketDTO>>> GetAllCustomerCareTickets(Partner partner, int pageNumber, int pageSize);

        Task<CustomerCareTicketDTO> GetCustomerCareTicketById(int id, Partner partner);

        Task<GeneralResponse> CreateCustomerCareTicket(CustomerCareTicketDTO customerCareTicketDTO, Employee employee, Partner partner);


        Task<GeneralResponse> UpdateCustomerCareTicket(int id, CustomerCareTicketDTO customerCareTicketDTO, Employee employee, Partner partner);

        Task<GeneralResponse> DeleteCustomerCareTicket(int id, Partner partner);

        public Task<GeneralResponse?> DeleteBulkCustomerTicketsAsync(
    string ids,
    Employee employee,
    Partner partner
);
        Task<GeneralResponse> BulkAddSupportTicketFromId(List<int> supportTicketIds, int id, Employee employee, Partner partner);
        Task<GeneralResponse> UnassignActivityFromId(int id, int activityId, Partner partner);

        Task<GeneralResponse> UnassignSupportTicketFromId(int id, int supportTicketId, Partner partner);

        //  ** Activity
        Task<List<ActivityDTO>> GetAllActivitiesByCustomerCareTickets(int id, Partner partner);
        Task<List<ActivityDTO>> GetAllActivitiesDoneByCustomerCareTickets(int id, Partner partner);

        Task<List<SupportTicketDTO>> GetAllSupportTicketsByCustomerCareTickets(int id, Partner partner);

    }
}