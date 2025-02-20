using Data.Enums;

namespace Data.Entities
{
    public class InvoiceEmployees
    {
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

        public AccessLevel AccessLevel { get; set; }

    }
}
