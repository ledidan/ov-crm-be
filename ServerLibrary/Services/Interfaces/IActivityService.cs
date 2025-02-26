using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IActivityService
    {
        public Task<List<Activity>> GetAllActivityAsync(Employee employee, Partner partner);

        Task<GeneralResponse> CreateAppointmentAsync(CreateAppointmentDTO appointmentDTO, Employee employee, Partner partner);
        Task<GeneralResponse> CreateMissionAsync(CreateMissionDTO activity, Employee employee, Partner partner);
        Task<GeneralResponse> CreateCallAsync(CreateCallDTO callDTO, Employee employee, Partner partner);

        Task<Activity?> GetByIdAsync(int id, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateActivityIdAsync(int id, UpdateActivityDTO updateActivityDTO, Employee employee, Partner partner);
        Task<GeneralResponse?> UpdateAppointmentByIdAsync(int id, UpdateAppointmentDTO updateActivityDTO, Employee employee, Partner partner);
        Task<GeneralResponse?> UpdateMissionByIdAsync(int id, UpdateMissionDTO updateActivityDTO, Employee employee, Partner partner);
        Task<GeneralResponse?> UpdateCallByIdAsync(int id, UpdateCallDTO updateActivityDTO, Employee employee, Partner partner);

        Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateActivityDTO updateActivityDTO, Employee employee, Partner partner);

        Task<GeneralResponse?> DeleteBulkActivities(string ids, Employee employee, Partner partner);

        Task<GeneralResponse?> DeleteIdAsync(int id, Employee employee, Partner partner);

    }
}