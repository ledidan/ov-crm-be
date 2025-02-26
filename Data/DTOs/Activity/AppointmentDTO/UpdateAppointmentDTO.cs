
using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class UpdateAppointmentDTO
    {
        public string? TagID { get; set; }
        public string? TagColor { get; set; }
        public bool? IsDeleted { get; set; }
        [Required]
        public string? ActivityName { get; set; }
        public string? ActivityCategory { get; set; }
        public DateTime? DueDate { get; set; }
        public string? StatusID { get; set; }
        public string? PriorityID { get; set; }
        public bool? IsSendNotificationEmail { get; set; }
        public bool? IsRepeat { get; set; }
        public bool? IsReminder { get; set; }
        public string? Description { get; set; }
        [Required]
        public DateTime? EventStart { get; set; }
        [Required]
        public DateTime EventEnd { get; set; }
        public string? Place { get; set; }
        public bool? Duplicate { get; set; }
        public bool? SendEmail { get; set; }
        public string? SearchTagID { get; set; }
        public bool? IsPublic { get; set; }

        public double? Lat { get; set; }
        public double? Long { get; set; }
        public bool? IsOpen { get; set; }
         public bool? IsAllDay { get; set; }
        public double? Distance { get; set; }
        public int? CustomerId { get; set; }
        public int? TaskOwnerId { get; set; }
        public int? ModifiedBy { get; set; }
        public int? ContactId { get; set; }
        public int? RelatedUsersID { get; set; }
    }
}