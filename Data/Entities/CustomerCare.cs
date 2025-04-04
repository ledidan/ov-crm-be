namespace Data.Entities
{
    public class CustomerCare : BaseEntity
    {
        public int Id { get; set; }

        public string? CustomerCareNumber { get; set; }

        public string? AccountNumber { get; set; }

        public string? TaxCode { get; set; }

        public string? Mobile { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? OwnerName { get; set; }

        public int? CustomerId { get; set; }

        public string? PartnerName { get; set; }

        public string? ReasonID { get; set; }

        public string? AccountCode { get; set; }

        public string? AccountName { get; set; }

        public string? Description { get; set; }

        public string? RateID { get; set; }

        public DateTime? SubscriptionDate { get; set; }

        public int? RelatedUsersID { get; set; }

        public string? DistrictID { get; set; }

        public string? CountryID { get; set; }

        public string? ProvinceID { get; set; }

        public string? WardID { get; set; }

        public string? SaleOrderID { get; set; }

        public string? TagID { get; set; }

        public string? SubscriptionStatusID { get; set; }

        public DateTime? CelebrateDate { get; set; }

        public int? CreatedBy { get; set; }

        public int? ModifiedBy { get; set; }

        public string? CreatedByName { get; set; }

        public string? ModifiedByName { get; set; }

        public virtual required Partner Partner { get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
}
