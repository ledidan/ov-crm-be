using Data.Enums;

namespace Data.Entities
{
    public class OrderEmployees
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

        public AccessLevel AccessLevel { get; set; }

    }
}
