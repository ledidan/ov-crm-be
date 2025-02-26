
namespace Data.DTOs
{
    public class UpdateActivityDTO
    {
        public string? TagID { get; set; }
        public string? TagColor { get; set; }
        public bool? IsDeleted { get; set; }
        public string? ActivityName { get; set; }
        public string? MissionName { get; set; }
        public string? MissionTypeID { get; set; }
        public string? ActivityCategory { get; set; }
        public DateTime? DueDate { get; set; }
        public string? StatusID { get; set; }
        public string? PriorityID { get; set; }
        public bool? IsSendNotificationEmail { get; set; }
        public bool? IsRepeat { get; set; }
        public bool? IsReminder { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? CallStart { get; set; }
        public int? CallDuration { get; set; }
        public string? Description { get; set; }
        public string? CallName { get; set; }
        public string? CallGoalID { get; set; }
        public string? CallTypeID { get; set; }
        public bool? CallDone { get; set; }

        public string? ModuleType { get; set; }
        public string? CallResult { get; set; }
        public string? RemindID { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public string? Place { get; set; }
        public bool? Duplicate { get; set; }
        public bool? SendEmail { get; set; }
        public string? SearchTagID { get; set; }
        public bool? IsPublic { get; set; }
        public double? Lat { get; set; }
        public double? Long { get; set; }
        public string? EventCheckinComment { get; set; }
        public DateTime? EventCheckinTime { get; set; }
        public string? CheckInAddress { get; set; }
        public string? CallID { get; set; }
        public string? CallRecord { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? WorkDuration { get; set; }
        public bool? IsOpen { get; set; }
        public double? Distance { get; set; }
        public string? BatteryStatus { get; set; }
        public string? RouteAddress { get; set; }
        public DateTime? CallEnd { get; set; }
        public string? EventCalendarID { get; set; }
        public string? Journey { get; set; }
        public bool? IsCorrectRoute { get; set; }
        public string? CheckOutAddress { get; set; }
        public bool? IsFakeGPS { get; set; }
        public string? CallResultID { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsCheckOutImages { get; set; }
        public string? ProviderName { get; set; }
        public string? RoutingResultID { get; set; }
        public double? TravelDistance { get; set; }
        public string? RoutingTypeID { get; set; }
        public string? RoutingTypeIDText { get; set; }
        public string? CheckinPlace { get; set; }
        public string? CheckOutPlace { get; set; }
        public bool? IsStartActivity { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountTel { get; set; }
        public string? OfficeEmail { get; set; }
        public string? CheckInType { get; set; }
        public int? CustomerId { get; set; }
        public int PartnerId { get; set; }
        public int? TaskOwnerId { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ContactId { get; set; }
        public int? RelatedUsersID { get; set; }
        // public List<Employee> Employees { get; set; } = new List<Employee>();
        // public ICollection<ActivityEmployees> ActivityEmployees { get; set; } = new List<ActivityEmployees>();

    }
}