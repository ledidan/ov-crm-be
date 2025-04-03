
using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class UpdateActivityDTO
    {   
        public int Id { get; set; }
        public string? ActivityName { get; set; }
        public string? ActivityCategory { get; set; }
        public DateTime? DueDate { get; set; }
        public string? StatusID { get; set; }
        public string? PriorityID { get; set; }
        public bool? IsSendNotificationEmail { get; set; }
        public bool? IsRepeat { get; set; }
        public bool? IsReminder { get; set; }
        public string? Description { get; set; }
        public string? ModuleType { get; set; }
        public string? RemindID { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public string? Place { get; set; }

        public bool? Duplicate { get; set; }
        public bool? SendEmail { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsOpen { get; set; }
        public bool? IsAllDay { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OfficeEmail { get; set; }
        public int? CustomerId { get; set; }
        [Required]
        public string? CustomerName { get; set; }
        public string? PartnerName { get; set; }
        public int? TaskOwnerId { get; set; }
        public string? TaskOwnerName { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public int? OrderId { get; set; }
        public int? InvoiceId { get; set; }
        public int? RelatedUsersID { get; set; }
        public string? RelatedUsersName { get; set; }
    }
}