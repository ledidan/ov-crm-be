using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.MongoModels;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IQuoteService
    {
        Task<DataObjectResponse?> GenerateQuoteCodeAsync(Partner partner);
        Task<DataObjectResponse?> CheckQuoteCodeAsync(string code, Employee employee, Partner partner);

        Task<List<OptionalQuoteDTO>> GetAllQuotesAsync(Employee employee, Partner partner);

        Task<QuoteDTO?> GetQuoteByIdAsync(int id, Employee employee, Partner partner);


        Task<GeneralResponse?> CreateQuoteAsync(
            CreateQuoteDTO quoteDto,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> UpdateFieldIdAsync(
            int id,
            UpdateQuoteDTO quotes,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> UpdateQuoteAsync(
            int id,
            UpdateQuoteDTO quotes,
            Employee employee,
            Partner partner
        );

        Task<GeneralResponse?> DeleteBulkQuotesAsync(
            string ids,
            Employee employee,
            Partner partner
        );

        Task<List<ActivityDTO?>> GetAllActivitiesByQuoteAsync(
            int id,
            Employee employee,
            Partner partner
        );
        Task<GeneralResponse> UnassignActivityFromId(int id, int activityId, Partner partner);

        Task<List<ActivityDTO>> GetAllActivitiesDoneByQuoteAsync(
            int id,
            Employee employee,
            Partner partner
        );
        Task<List<OptionalOrderDTO>> GetAllOrdersByQuoteAsync(
            int id,
            Employee employee,
            Partner partner
        );
    }
}
