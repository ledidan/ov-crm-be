namespace Data.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string? CivilId { get; set; }
        public string? Fullname { get; set; }
        public string? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? JobTitle { get; set; }
        public string? Email { get; set; }
        public string? StreetAddress { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? TaxIdentificationNumber { get; set; }
        public DateTime SignedContractDate { get; set; }
        public virtual required Partner Partner { get; set; }
    }
}
