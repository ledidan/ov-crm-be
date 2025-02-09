using Data.Enums;

namespace Data.Entities
{
    public class ProductEmployees
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

        public AccessLevel AccessLevel { get; set; }

    }
}
