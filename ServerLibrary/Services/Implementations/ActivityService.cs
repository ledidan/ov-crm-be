using AutoMapper;
using Data.DTOs;
using Data.DTOs.Contact;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class ActivityService : IActivityService
    {
        private readonly AppDbContext _appDbContext;

        private readonly IPartnerService _partnerService;

        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        public ActivityService(AppDbContext appDbContext, IMapper mapper,
        IPartnerService partnerService, IEmployeeService employeeService
        )
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }

        public async Task<GeneralResponse?> DeleteBulkActivities(string ids, Employee employee, Partner partner)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return new GeneralResponse(false, "Không có hoạt động nào được cung cấp để xóa");
            }

            var idList = ids.Split(',')
                            .Select(id => int.TryParse(id, out int parsedId) ? parsedId : (int?)null)
                            .Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .ToList();

            if (!idList.Any())
            {
                return new GeneralResponse(false, "ID hoạt động không hợp lệ");
            }

            var activities = await _appDbContext.Activities
                .Where(a => idList.Contains(a.Id) && a.PartnerId == partner.Id)
                .ToListAsync();

            if (!activities.Any())
            {
                return new GeneralResponse(false, "Không tìm thấy hoạt động");
            }

            // var unauthorizedActivities = activities
            //     .Where(a => a.TaskOwnerId != employee.Id)
            //     .ToList();

            // if (unauthorizedActivities.Any())
            // {
            //     return new GeneralResponse(false, "Bạn không có quyền xóa một số hoạt động");
            // }

            _appDbContext.Activities.RemoveRange(activities);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Xóa hoạt động thành công");
        }

        public async Task<GeneralResponse?> DeleteIdAsync(int id, Employee employee, Partner partner)
        {
            if (id == null)
            {
                return new GeneralResponse(false, "Không có hoạt động nào được cung cấp để xóa");
            }

            var activity = await _appDbContext.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.PartnerId == partner.Id);

            if (activity == null)
            {
                return new GeneralResponse(false, "Không tìm thấy hoạt động");
            }

            // var unauthorizedActivities = activities
            //     .Where(a => a.TaskOwnerId != employee.Id)
            //     .ToList();

            // if (unauthorizedActivities.Any())
            // {
            //     return new GeneralResponse(false, "Bạn không có quyền xóa một số hoạt động");
            // }

            _appDbContext.Activities.Remove(activity);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Xóa hoạt động thành công");
        }

        public async Task<List<Activity>> GetAllActivityAsync(Partner partner)
        {
            if (partner == null)
            {
                throw new ArgumentNullException("Partner không được null.");
            }
            try
            {
                var activities = await _appDbContext.Activities
             .Include(a => a.Appointment)
             .Include(a => a.Mission)
             .Include(a => a.Call)
             .Where(a => a.PartnerId == partner.Id &&
                         (a.Appointment != null || a.Mission != null || a.Call != null))
             .ToListAsync();

                return activities.Any() ? activities : new List<Activity>();

            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu tất cả hoạt động: {ex.Message}");
            }
        }

        public async Task<ActivityResponseDTO?> GetByIdAsync(int id, Partner partner)
        {
            if (partner == null || await _partnerService.FindById(partner.Id) == null)
            {
                return null;
            }

            var activity = await _appDbContext.Activities
                .Include(a => a.Appointment)
                .Include(a => a.Call)
                .Include(a => a.Mission)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activity == null)
            {
                return null;
            }

            return new ActivityResponseDTO
            {
                Activity = _mapper.Map<ActivityDTO>(activity),
                Appointment = activity.Appointment != null ? _mapper.Map<AppointmentDTO>(activity.Appointment) : null,
                Call = activity.Call != null ? _mapper.Map<CallDTO>(activity.Call) : null,
                Mission = activity.Mission != null ? _mapper.Map<MissionDTO>(activity.Mission) : null
            };
        }

        public async Task<GeneralResponse?> UpdateActivityIdAsync(int id, UpdateActivityDTO dto, Partner partner)
        {
            var activity = await _appDbContext.Activities.FindAsync(id);
            if (activity == null) return new GeneralResponse(false, "Không tìm thấy hoạt động");
            if (partner == null)
            {
                return new GeneralResponse(false, "Partner không được null.");
            }
            if (partner.Id != activity.PartnerId)
            {
                return new GeneralResponse(false, $"Bạn không có quyền cập nhật hoạt động này");
            }

            activity.ActivityName = dto.ActivityName ?? activity.ActivityName;
            activity.ActivityCategory = dto.ActivityCategory ?? activity.ActivityCategory;
            activity.DueDate = dto.DueDate ?? activity.DueDate;
            activity.StatusID = dto.StatusID ?? activity.StatusID;
            activity.PriorityID = dto.PriorityID ?? activity.PriorityID;
            activity.IsSendNotificationEmail = dto.IsSendNotificationEmail ?? activity.IsSendNotificationEmail;
            activity.IsRepeat = dto.IsRepeat ?? activity.IsRepeat;
            activity.IsReminder = dto.IsReminder ?? activity.IsReminder;
            activity.Description = dto.Description ?? activity.Description;
            activity.ModuleType = dto.ModuleType ?? activity.ModuleType;
            activity.RemindID = dto.RemindID ?? activity.RemindID;
            activity.EventStart = dto.EventStart ?? activity.EventStart;
            activity.EventEnd = dto.EventEnd;
            activity.Place = dto.Place ?? activity.Place;
            activity.Duplicate = dto.Duplicate ?? activity.Duplicate;
            activity.SendEmail = dto.SendEmail ?? activity.SendEmail;
            activity.IsPublic = dto.IsPublic ?? activity.IsPublic;
            activity.IsOpen = dto.IsPublic ?? activity.IsOpen;
            activity.IsAllDay = dto.IsAllDay ?? activity.IsAllDay;
            activity.PhoneNumber = dto.PhoneNumber ?? activity.PhoneNumber;
            activity.OfficeEmail = dto.OfficeEmail ?? activity.OfficeEmail;
            activity.CustomerId = dto.CustomerId ?? activity.CustomerId;
            activity.CustomerName = dto.CustomerName ?? activity.CustomerName;
            activity.ContactId = dto.ContactId ?? activity.ContactId;
            activity.ContactName = dto.ContactName ?? activity.ContactName;
            activity.TaskOwnerId = dto.TaskOwnerId ?? activity.TaskOwnerId;
            activity.TaskOwnerName = dto.TaskOwnerName ?? activity.TaskOwnerName;
            activity.ModifiedBy = dto.ModifiedBy ?? activity.ModifiedBy;
            activity.ModifiedByName = dto.ModifiedByName ?? activity.ModifiedByName;
            activity.OrderId = dto.OrderId ?? activity.OrderId;
            activity.InvoiceId = dto.InvoiceId ?? activity.InvoiceId;
            activity.RelatedUsersID = dto.RelatedUsersID ?? activity.RelatedUsersID;
            activity.RelatedUsersName = dto.RelatedUsersName ?? activity.RelatedUsersName;

            await _appDbContext.SaveChangesAsync();
            return new GeneralResponse(true, "Đã cập nhật hành động");
        }

        public async Task<GeneralResponse?> UpdateAppointmentByIdAsync(int activityId, UpdateActivityDTO activityDto,
         UpdateAppointmentDTO updateAppointmentDTO, Partner partner)
        {
            var activityResponse = await UpdateActivityIdAsync(activityId, activityDto, partner);
            if (!activityResponse.Flag) return activityResponse;

            var appointment = await _appDbContext.Appointments.FirstOrDefaultAsync(a => a.ActivityId == activityId);

            if (appointment == null)
                return new GeneralResponse(false, "Không tìm thấy lịch hẹn trong của hoạt động");

                
            if (updateAppointmentDTO != null)
            {
                appointment.IsAllDay = updateAppointmentDTO.IsAllDay;
                _mapper.Map(updateAppointmentDTO, appointment);
            }
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Cập nhật lịch hẹn thành công");
        }

        public async Task<GeneralResponse?> UpdateCallByIdAsync(int activityId, UpdateActivityDTO activityDto,
         UpdateCallDTO updateCallDTO, Partner partner)
        {
            var activityResponse = await UpdateActivityIdAsync(activityId, activityDto, partner);
            if (!activityResponse.Flag) return activityResponse;

            var call = await _appDbContext.Calls.FirstOrDefaultAsync(c => c.ActivityId == activityId);
            if (call == null)
                return new GeneralResponse(false, "Không tìm thấy cuộc gọi của hoạt động");

            _mapper.Map(updateCallDTO, call);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Cập nhật cuộc gọi thành công");
        }

        public async Task<GeneralResponse?> UpdateMissionByIdAsync(int activityId, UpdateActivityDTO activityDto,
        UpdateMissionDTO updateMissionDTO, Partner partner)
        {
            var activityResponse = await UpdateActivityIdAsync(activityId, activityDto, partner);
            if (!activityResponse.Flag) return activityResponse;

            var mission = await _appDbContext.Missions.FirstOrDefaultAsync(m => m.ActivityId == activityId);
            if (mission == null)
                return new GeneralResponse(false, "Không tìm thấy nhiệm vụ của hoạt động");

            _mapper.Map(updateMissionDTO, mission);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Cập nhật nhiệm vụ thành công");
        }

        public async Task<ActivityDTO> CreateActivityAsync(CreateActivityDTO dto, string ModuleType, Partner partner)
        {
            var activity = new Activity
            {
                ActivityName = dto.ActivityName,
                ActivityCategory = dto.ActivityCategory,
                DueDate = dto.DueDate,
                StatusID = dto.StatusID,
                PriorityID = dto.PriorityID,
                IsSendNotificationEmail = dto.IsSendNotificationEmail,
                IsRepeat = dto.IsRepeat,
                IsReminder = dto.IsReminder,
                Description = dto.Description,
                ModuleType = ModuleType,
                RemindID = dto.RemindID,
                EventStart = dto.EventStart,
                EventEnd = dto.EventEnd,
                Place = dto.Place,
                Duplicate = dto.Duplicate,
                SendEmail = dto.SendEmail,
                IsPublic = dto.IsPublic,
                IsOpen = dto.IsPublic,
                IsAllDay = dto.IsAllDay,
                PhoneNumber = dto.PhoneNumber,
                OfficeEmail = dto.OfficeEmail,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                ContactId = dto.ContactId,
                ContactName = dto.ContactName,
                TaskOwnerId = dto.TaskOwnerId,
                TaskOwnerName = dto.TaskOwnerName,
                ModifiedBy = dto.ModifiedBy,
                ModifiedByName = dto.ModifiedByName,
                OrderId = dto.OrderId,
                InvoiceId = dto.InvoiceId,
                RelatedUsersID = dto.RelatedUsersID,
                RelatedUsersName = dto.RelatedUsersName,
                PartnerId = partner.Id,
                PartnerName = partner.Name
            };
            await _appDbContext.InsertIntoDb(activity);

            return new ActivityDTO { Id = activity.Id, ActivityName = activity.ActivityName };
        }

        public async Task<GeneralResponse> CreateAppointmentAsync(CreateActivityDTO activityDto, CreateAppointmentDTO appointmentDto, Partner partner)
        {
            var ModuleType = ActivityModuleType.Appointment.ToString();
            var createdActivity = await CreateActivityAsync(activityDto, ModuleType, partner);

            var appointment = new Appointment
            {
                ActivityId = createdActivity.Id,
                IsAllDay = appointmentDto.IsAllDay
            };

            await _appDbContext.Appointments.AddAsync(appointment);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Tạo lịch hẹn thành công");
        }

        public async Task<GeneralResponse> CreateMissionAsync(CreateActivityDTO activityDto, CreateMissionDTO mission, Partner partner)
        {
            var ModuleType = ActivityModuleType.Mission.ToString();
            var createdActivity = await CreateActivityAsync(activityDto, ModuleType, partner);

            var Mission = new Mission
            {
                ActivityId = createdActivity.Id,
                MissionName = createdActivity.ActivityName,
                MissionTypeID = mission.MissionTypeID
            };
            await _appDbContext.Missions.AddAsync(Mission);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Tạo nhiệm vụ thành công");
        }

        public async Task<GeneralResponse> CreateCallAsync(CreateActivityDTO activityDto, CreateCallDTO callDTO, Partner partner)
        {
            var ModuleType = ActivityModuleType.Call.ToString();
            var createdActivity = await CreateActivityAsync(activityDto, ModuleType, partner);

            var Call = new Call
            {
                ActivityId = createdActivity.Id,
                CallStart = callDTO.CallStart,
                CallDuration = callDTO.CallDuration,
                CallName = createdActivity.ActivityName,
                CallGoalID = callDTO.CallGoalID,
                CallTypeID = callDTO.CallTypeID,
                CallDone = callDTO.CallDone,
                CallResult = callDTO.CallResult,
                CallID = callDTO.CallID,
                CallRecord = callDTO.CallRecord,
                CallEnd = callDTO.CallEnd,
                CallResultID = callDTO.CallResultID,
                PhoneNumber = callDTO.PhoneNumber,

            };
            await _appDbContext.Calls.AddAsync(Call);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Tạo cuộc gọi thành công");
        }
    }
}