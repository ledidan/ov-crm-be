
namespace Data.DTOs
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public bool? IsAllDay { get; set; }
    }
}