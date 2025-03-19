namespace Data.Entities
{
    public class Mission
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public string? MissionName { get; set; }
        public string? MissionTypeID { get; set; }
        public Activity Activity { get; set; } = null!;
    }
}