


namespace Data.DTOs
{

    public class SupportTicketDTO
    {
        public int Id { get; set; }

        public string? TicketNumber { get; set; }

        public string? Question { get; set; }

        public string? Response { get; set; }

        public string? OwnerName { get; set; }

        public int? OwnerId { get; set; }

        public int? CustomerId { get; set; }

        public string? AccountName { get; set; }

        public string? AccountNumber { get; set; }

        public int? ContactId { get; set; }

        public string? ContactName { get; set; }

        public string? Address { get; set; }

        public string? Description { get; set; }

        public string? StatusID { get; set; }

        public string? ReasonID { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public string? TaxCode { get; set; }

        public string? PriorityID { get; set; }

        public string? SourceID { get; set; }

        public int? ProductCategoryId { get; set; }

        public int? CustomerCareCardId { get; set; }

        public string? RequirementTypeID { get; set; }

        public DateTime? ProcessStartDate { get; set; }

        public DateTime? ProcessEndDate { get; set; }

        public bool? Inactive { get; set; } = false;

        public int? RelatedUsersID { get; set; }

        public int? ModifiedBy { get; set; }

        public int? CreatedBy { get; set; }

        public string? ModifiedByName { get; set; }

        public string? CreatedByName { get; set; }

    }

}