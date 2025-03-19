namespace Data.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public bool? IsAllDay { get; set; }
        public int ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;
    }
}