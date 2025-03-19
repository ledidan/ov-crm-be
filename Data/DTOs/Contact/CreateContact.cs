
using System.ComponentModel.DataAnnotations;
using Data.Entities;

namespace Data.DTOs
{
    public class CreateContact
    {

        public string? ContactCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tên liên hệ không được để trống !")]
        public string? ContactName { get; set; }
        [Required(ErrorMessage = "Tên không được để trống !")]
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? OfficeTel { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Description { get; set; }
        public string? ShippingAddress { get; set; }
        public string? AccountTypeID { get; set; }
        public string? DepartmentID { get; set; }
        public string? LeadSourceID { get; set; }
        public string? MailingDistrictID { get; set; }
        public string? MailingProvinceID { get; set; }
        public string? MailingStreet { get; set; }
        public string? MailingWardID { get; set; }
        public string? MailingZip { get; set; }
        public string? Mobile { get; set; }
        public string? OfficeEmail { get; set; }
        public string? OtherPhone { get; set; }
        public string? SalutationID { get; set; }
        public string? ShippingDistrictID { get; set; }
        public string? ShippingProvinceID { get; set; }
        public string? ShippingStreet { get; set; }
        public string? ShippingWardID { get; set; }
        public string? ShippingZip { get; set; }
        public string? TitleID { get; set; }
        public string? Zalo { get; set; }
        public bool? IsPublic { get; set; } = false;
        public bool? EmailOptOut { get; set; } = false;
        public bool? PhoneOptOut { get; set; } = false;
        public int PartnerId { get; set; }
        public int? EmployeeId { get; set; }
    }
}