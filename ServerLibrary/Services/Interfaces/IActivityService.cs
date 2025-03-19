using Data.DTOs;
using Data.Entities;
using Data.Responses;

namespace ServerLibrary.Services.Interfaces
{
    public interface IActivityService
    {
        // ** Handle for main Activity first
        Task<List<Activity>> GetAllActivityAsync(Partner partner);
        Task<ActivityResponseDTO?> GetByIdAsync(int id, Partner partner);
        Task<ActivityDTO> CreateActivityAsync(CreateActivityDTO dto, string ModuleType, Partner partner);
        Task<GeneralResponse?> UpdateActivityIdAsync(int id, UpdateActivityDTO dto, Partner partner);
        Task<GeneralResponse?> DeleteBulkActivities(string ids, Employee employee, Partner partner);
        Task<GeneralResponse?> DeleteIdAsync(int id, Employee employee, Partner partner);

        // ______

        // ** Appointment
        Task<GeneralResponse> CreateAppointmentAsync(CreateActivityDTO activityDto, CreateAppointmentDTO appointmentDto, Partner partner);
        Task<GeneralResponse?> UpdateAppointmentByIdAsync(int activityId, UpdateActivityDTO activityDto, UpdateAppointmentDTO updateActivityDTO, Partner partner);

        // ** Mission
        Task<GeneralResponse> CreateMissionAsync(CreateActivityDTO activityDto, CreateMissionDTO mission, Partner partner);
        Task<GeneralResponse?> UpdateMissionByIdAsync(int activityId, UpdateActivityDTO activityDto, UpdateMissionDTO updateActivityDTO, Partner partner);

        // ** Call
        Task<GeneralResponse> CreateCallAsync(CreateActivityDTO activityDto, CreateCallDTO callDTO, Partner partner);

        Task<GeneralResponse?> UpdateCallByIdAsync(int activityId, UpdateActivityDTO activityDto, UpdateCallDTO updateActivityDTO, Partner partner);


    }
}