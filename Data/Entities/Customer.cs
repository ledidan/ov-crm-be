using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? StreetAddress { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }

    }
}
