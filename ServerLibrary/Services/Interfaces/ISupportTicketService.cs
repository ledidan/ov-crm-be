using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface ISupportTicketService
    {
        public Task<List<SupportTicketDTO>> GetAllSupportTickets(Partner partner);

        public Task<SupportTicketDTO> GetSupportTicketById(int id, Partner partner);

        public Task<GeneralResponse> CreateSupportTicket(SupportTicketDTO supportTicketDTO, Employee employee, Partner partner);

        public Task<GeneralResponse> UpdateSupportTicket(int id, SupportTicketDTO supportTicketDTO, Employee employee, Partner partner);

        public Task<GeneralResponse> DeleteSupportTicket(int id, Partner partner);
        public Task<GeneralResponse?> DeleteBulkTicketsAsync(
        string ids,
        Employee employee,
        Partner partner
    );

        Task<GeneralResponse> UnassignActivityFromId(int id, int activityId, Partner partner);

        //  ** Activity
        Task<List<ActivityDTO>> GetAllActivitiesBySupportTickets(int id, Partner partner);
        Task<List<ActivityDTO>> GetAllActivitiesDoneBySupportTickets(int id, Partner partner);
    }
}