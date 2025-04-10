


using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CustomerCareTicketDTO
    {
        public int Id { get; set; }

        [Required]
        public string? CustomerCareNumber { get; set; }

        public string? AccountNumber { get; set; }

        public string? TaxCode { get; set; }

        public string? Mobile { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? OwnerName { get; set; }
        [Required]

        public int? CustomerId { get; set; }

        public string? PartnerName { get; set; }

        public string? ReasonID { get; set; }

        public string? AccountName { get; set; }

        public string? Description { get; set; }

        public string? RateID { get; set; }

        public DateTime? SubscriptionDate { get; set; }

        public int? RelatedUsersID { get; set; }

        public string? DistrictID { get; set; }

        public string? CountryID { get; set; }

        public string? ProvinceID { get; set; }

        public string? WardID { get; set; }

        [Required]

        public int? SaleOrderID { get; set; }

        public string? SaleOrderNo { get; set; }

        public string? TagID { get; set; }

        public string? SubscriptionStatusID { get; set; }

        public DateTime? CelebrateDate { get; set; }

        public int? CreatedBy { get; set; }

        public int? ModifiedBy { get; set; }

        public string? CreatedByName { get; set; }

        public string? ModifiedByName { get; set; }


    }
}