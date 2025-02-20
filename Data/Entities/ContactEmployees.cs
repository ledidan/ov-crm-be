using Data.Enums;

namespace Data.Entities
{
    public class ContactEmployees
    {
        public int ContactId { get; set; }
        public Contact Contact { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

        public AccessLevel AccessLevel { get; set; }

    }
}
