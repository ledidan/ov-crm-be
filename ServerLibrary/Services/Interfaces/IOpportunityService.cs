using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IOpportunityService
    {
        Task<DataObjectResponse?> GenerateOpportunityCodeAsync(Partner partner);
        Task<DataObjectResponse?> CheckOpportunityCodeAsync(string code, Employee employee, Partner partner);

        Task<List<OptionalOpportunityDTO>> GetAllOpportunitiesAsync(Employee employee, Partner partner);

        Task<OpportunityDTO?> GetOpportunityByIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse?> CreateOpportunityAsync(
            CreateOpportunityDTO opportunityDto,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> UpdateFieldIdAsync(
            int id,
            UpdateOpportunityDTO opportunities,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> UpdateOpportunityAsync(
            int id,
            UpdateOpportunityDTO opportunities,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> DeleteBulkOpportunitiesAsync(
            string ids,
            Employee employee,
            Partner partner
        );

        Task<List<Activity?>> GetAllActivitiesByOpportunityAsync(
            int opportunityId,
            Employee employee,
            Partner partner
        );

        Task<List<ContactDTO>> GetAllContactsAvailableByIdAsync(
            int opportunityId,
            Employee employee,
            Partner partner
        );
        // ** Bulk Add Opportunity Relationship
        Task<GeneralResponse?> BulkAddContactsIntoId(List<int> contactIds, int opportunityId, Employee employee, Partner partner);

        Task<GeneralResponse?> RemoveContactFromId(int id, int contactId, Partner partner);

        Task<GeneralResponse?> UnassignActivityFromId(int id, int activityId, Partner partner);

        Task<GeneralResponse?> UnassignOrderFromId(int id, int orderId, Partner partner);
        
        Task<GeneralResponse?> UnassignQuoteFromId(int id, int quoteId, Partner partner);

        //  ** Bulk Get Opportunity Relationship

        Task<List<ContactDTO>> GetAllContactsByIdAsync(int id, Partner partner);

        Task<List<ContactDTO>> GetAllContactAvailableById(int id, Partner partner);
        Task<List<Activity>> GetAllActivitiesByIdAsync(int id, Partner partner);

        Task<List<OptionalOrderDTO>> GetAllOrdersByIdAsync(int id, Partner partner);

        Task<List<QuoteDTO>> GetAllQuotesByIdAsync(int id, Partner partner);



    }
}