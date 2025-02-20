using Data.Enums;

namespace Data.Entities
{
    public class CustomerEmployees
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

        public AccessLevel AccessLevel { get; set; }

    }
}
