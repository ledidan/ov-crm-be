

namespace Data.DTOs
{
    public class CreateCallDTO
    {
        public DateTime? CallStart { get; set; }
        public int? CallDuration { get; set; }
        public string? CallName { get; set; }
        public string? CallGoalID { get; set; }
        public string? CallTypeID { get; set; }
        public bool? CallDone { get; set; }
        public string? CallResult { get; set; }
        public string? CallID { get; set; }
        public string? CallRecord { get; set; }
        public DateTime? CallEnd { get; set; }
        public string? CallResultID { get; set; }
        public string? PhoneNumber { get; set; }

    }
}