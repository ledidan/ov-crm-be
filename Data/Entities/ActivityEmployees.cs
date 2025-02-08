using Data.Enums;

namespace Data.Entities
{
    public class ActivityEmployees
    {
        public int ActivityId { get; set; }
        public Activity Activity { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

        public AccessLevel AccessLevel { get; set; }

    }
}
