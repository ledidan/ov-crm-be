using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        public string? CivilId { get; set; }
        public string? Fullname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }

        public string? StreetAddress { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
    }
}
