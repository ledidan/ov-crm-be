namespace Data.Entities
{
    public class Call
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
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
        public Activity Activity { get; set; } = null!;
    }
}