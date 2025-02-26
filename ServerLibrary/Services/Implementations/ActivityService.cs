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

        public async Task<GeneralResponse> CreateAppointmentAsync(CreateAppointmentDTO appointmentDTO, Employee employee, Partner partner)
        {
            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
            {
                return new GeneralResponse(false, "Không tìm thấy tổ chức");
            }

            if (employee != null)
            {
                var employeeData = await _employeeService.FindByIdAsync(employee.Id);
                if (employeeData == null)
                {
                    return new GeneralResponse(false, "Không tìm thấy nhân viên");
                }
            }
            else
            {
                return new GeneralResponse(false, "ID Nhân viên không được để trống");
            }

            await _appDbContext.InsertIntoDb(new Activity()
            {
                TagID = appointmentDTO.TagID,
                TagColor = appointmentDTO.TagColor,
                IsDeleted = appointmentDTO.IsDeleted,
                ActivityName = appointmentDTO.ActivityName,
                ActivityCategory = appointmentDTO.ActivityCategory,
                DueDate = appointmentDTO.DueDate,
                StatusID = appointmentDTO.StatusID,
                PriorityID = appointmentDTO.PriorityID,
                IsSendNotificationEmail = appointmentDTO.IsSendNotificationEmail,
                IsRepeat = appointmentDTO.IsRepeat,
                ModuleType = ActivityModuleType.Appointment.ToString(),
                IsReminder = appointmentDTO.IsReminder,
                Description = appointmentDTO.Description,
                EventStart = appointmentDTO.EventStart,
                EventEnd = appointmentDTO.EventEnd,
                Place = appointmentDTO.Place,
                Duplicate = appointmentDTO.Duplicate,
                SendEmail = appointmentDTO.SendEmail,
                SearchTagID = appointmentDTO.SearchTagID,
                IsPublic = appointmentDTO.IsPublic,
                IsAllDay = appointmentDTO.IsAllDay,
                Lat = appointmentDTO.Lat,
                Long = appointmentDTO.Long,
                IsOpen = appointmentDTO.IsOpen,
                Distance = appointmentDTO.Distance,
                CustomerId = appointmentDTO.CustomerId,
                TaskOwnerId = appointmentDTO.TaskOwnerId,
                ModifiedBy = appointmentDTO.ModifiedBy,
                ContactId = appointmentDTO.ContactId,
                RelatedUsersID = appointmentDTO.RelatedUsersID,
                PartnerId = partner.Id,
            });
            return new GeneralResponse(true, "Tạo lịch hẹn thành công");
        }

        public async Task<GeneralResponse> CreateCallAsync(CreateCallDTO callDTO, Employee employee, Partner partner)
        {
            // Validate that the partner exists.
            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
            {
                return new GeneralResponse(false, "Không tìm thấy tổ chức");
            }

            // Validate that the employee exists.
            if (employee == null)
            {
                return new GeneralResponse(false, "ID Nhân viên không được để trống");
            }
            var employeeData = await _employeeService.FindByIdAsync(employee.Id);
            if (employeeData == null)
            {
                return new GeneralResponse(false, "Không tìm thấy nhân viên");
            }

            // Map properties from CreateCallDTO to a new call entity (CallDTO).
            // Adjust the mapping as needed if your domain entity differs.
            var newCall = new Activity
            {
                TagID = callDTO.TagID,
                TagColor = callDTO.TagColor,
                IsDeleted = callDTO.IsDeleted,
                DueDate = callDTO.DueDate,
                StatusID = callDTO.StatusID,
                PriorityID = callDTO.PriorityID,
                CallStart = callDTO.CallStart,
                CallDuration = callDTO.CallDuration,
                Description = callDTO.Description,
                ActivityName = callDTO.ActivityName,
                CallName = callDTO.ActivityName,
                ModuleType = ActivityModuleType.Call.ToString(),
                CallGoalID = callDTO.CallGoalID,
                CallTypeID = callDTO.CallTypeID,
                CallDone = callDTO.CallDone,
                CallResult = callDTO.CallResult,
                EventStart = callDTO.CallStart,
                EventEnd = callDTO.CallEnd,
                Duplicate = callDTO.Duplicate,
                SendEmail = callDTO.SendEmail,
                CallID = callDTO.CallID,
                CallRecord = callDTO.CallRecord,
                CallEnd = callDTO.CallEnd,
                CallResultID = callDTO.CallResultID,
                PhoneNumber = callDTO.PhoneNumber,
                CustomerId = callDTO.CustomerId,
                TaskOwnerId = callDTO.TaskOwnerId,
                ContactId = callDTO.ContactId,
                RelatedUsersID = callDTO.RelatedUsersID,
                ModifiedBy = employee.Id,
                PartnerId = partner.Id,
            };

            // Persist the new call using the repository.
            await _appDbContext.AddAsync(newCall);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Tạo cuộc gọi thành công");
        }

        public async Task<GeneralResponse> CreateMissionAsync(CreateMissionDTO missionDTO, Employee employee, Partner partner)
        {
            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
            {
                return new GeneralResponse(false, "Không tìm thấy tổ chức");
            }

            if (employee == null)
            {
                return new GeneralResponse(false, "ID Nhân viên không được để trống");
            }
            var employeeData = await _employeeService.FindByIdAsync(employee.Id);
            if (employeeData == null)
            {
                return new GeneralResponse(false, "Không tìm thấy nhân viên");
            }

            var newMission = new Activity
            {
                TagID = missionDTO.TagID,
                TagColor = missionDTO.TagColor,
                IsDeleted = missionDTO.IsDeleted,
                ActivityName = missionDTO.ActivityName,
                MissionName = missionDTO.ActivityName,
                MissionTypeID = missionDTO.MissionTypeID,
                DueDate = missionDTO.DueDate,
                StatusID = missionDTO.StatusID,
                ModuleType = ActivityModuleType.Mission.ToString(),
                PriorityID = missionDTO.PriorityID,
                IsRepeat = missionDTO.IsRepeat,
                IsReminder = missionDTO.IsReminder,
                Description = missionDTO.Description,
                Duplicate = missionDTO.Duplicate,
                SendEmail = missionDTO.SendEmail,
                IsPublic = missionDTO.IsPublic,
                CustomerId = missionDTO.CustomerId,
                TaskOwnerId = missionDTO.TaskOwnerId,
                ModifiedBy = missionDTO.ModifiedBy,
                ContactId = missionDTO.ContactId,
                RelatedUsersID = missionDTO.RelatedUsersID,
                PartnerId = partner.Id
            };

            await _appDbContext.AddAsync(newMission);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Tạo nhiệm vụ thành công");
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
        
            var unauthorizedActivities = activities
                .Where(a => a.TaskOwnerId != employee.Id)
                .ToList();

            if (unauthorizedActivities.Any())
            {
                return new GeneralResponse(false, "Bạn không có quyền xóa một số hoạt động");
            }

            _appDbContext.Activities.RemoveRange(activities);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Xóa hoạt động thành công");
        }

        public Task<GeneralResponse?> DeleteIdAsync(int id, Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Activity>> GetAllActivityAsync(Employee employee, Partner partner)
        {
            try
            {
                var activities = await _appDbContext.Activities
                                  .Where(a =>
                                      a.PartnerId == partner.Id && a.TaskOwnerId == employee.Id)
                                  .ToListAsync();


                return activities.Any() ? activities : new List<Activity>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy dữ liệu tất cả hoạt động: {ex.Message}");
            }
        }

        public async Task<Activity?> GetByIdAsync(int id, Employee employee, Partner partner)
        {
            if (employee == null || partner == null)
            {
                return null;
            }

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
            {
                return null;
            }

            var employeeData = await _employeeService.FindByIdAsync(employee.Id);
            if (employeeData == null)
            {
                return null;
            }

            var activity = await _appDbContext.Activities.FirstOrDefaultAsync(c => c.Id == id
            && c.TaskOwnerId == employeeData.Id
            && c.PartnerId == partnerData.Id);

            return activity;
        }

        public Task<GeneralResponse?> UpdateActivityIdAsync(int id, UpdateActivityDTO updateActivityDTO, Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }

        public async Task<GeneralResponse?> UpdateAppointmentByIdAsync(int id, UpdateAppointmentDTO updateAppointmentDTO, Employee employee, Partner partner)
        {
            if (employee == null || partner == null)
                return new GeneralResponse(false, "Thông tin nhân viên hoặc tổ chức không được cung cấp");

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
                return new GeneralResponse(false, "Không tìm thấy tổ chức");

            var employeeData = await _employeeService.FindByIdAsync(employee.Id);
            if (employeeData == null)
                return new GeneralResponse(false, "Không tìm thấy nhân viên");

            var appointment = await _appDbContext.Activities.FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                return new GeneralResponse(false, "Không tìm thấy lịch hẹn");

            if (appointment.PartnerId != partner.Id || appointment.TaskOwnerId != employee.Id)
                return new GeneralResponse(false, "Bạn không có quyền cập nhật lịch hẹn này");

            _mapper.Map(updateAppointmentDTO, appointment);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Cập nhật lịch hẹn thành công");
        }

        public async Task<GeneralResponse?> UpdateCallByIdAsync(int id, UpdateCallDTO updateCallDTO, Employee employee, Partner partner)
        {
            if (employee == null || partner == null)
                return new GeneralResponse(false, "Thông tin nhân viên hoặc tổ chức không được cung cấp");

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
                return new GeneralResponse(false, "Không tìm thấy tổ chức");

            var employeeData = await _employeeService.FindByIdAsync(employee.Id);
            if (employeeData == null)
                return new GeneralResponse(false, "Không tìm thấy nhân viên");

            var call = await _appDbContext.Activities.FirstOrDefaultAsync(c => c.Id == id);
            if (call == null)
                return new GeneralResponse(false, "Không tìm thấy cuộc gọi");

            if (call.PartnerId != partner.Id || call.TaskOwnerId != employee.Id)
                return new GeneralResponse(false, "Bạn không có quyền cập nhật cuộc gọi này");

            _mapper.Map(updateCallDTO, call);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Cập nhật cuộc gọi thành công");
        }

        public async Task<GeneralResponse?> UpdateMissionByIdAsync(int id, UpdateMissionDTO updateMissionDTO, Employee employee, Partner partner)
        {
            if (employee == null || partner == null)
                return new GeneralResponse(false, "Thông tin nhân viên hoặc tổ chức không được cung cấp");

            var partnerData = await _partnerService.FindById(partner.Id);
            if (partnerData == null)
                return new GeneralResponse(false, "Không tìm thấy tổ chức");

            var employeeData = await _employeeService.FindByIdAsync(employee.Id);
            if (employeeData == null)
                return new GeneralResponse(false, "Không tìm thấy nhân viên");

            var mission = await _appDbContext.Activities.FirstOrDefaultAsync(m => m.Id == id);
            if (mission == null)
                return new GeneralResponse(false, "Không tìm thấy nhiệm vụ");

            if (mission.PartnerId != partner.Id || mission.TaskOwnerId != employee.Id)
                return new GeneralResponse(false, "Bạn không có quyền cập nhật nhiệm vụ này");

            _mapper.Map(updateMissionDTO, mission);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Cập nhật nhiệm vụ thành công");
        }

        public Task<GeneralResponse?> UpdateFieldIdAsync(int id, UpdateActivityDTO updateActivityDTO, Employee employee, Partner partner)
        {
            throw new NotImplementedException();
        }

    }
}