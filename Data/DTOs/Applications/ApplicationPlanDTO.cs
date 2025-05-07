


namespace Data.DTOs
{
    public class ApplicationPlanDTO
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Name { get; set; } // Basic, Pro, Enterprise
        public string Description { get; set; }
        public decimal PriceMonthly { get; set; }
        public decimal PriceYearly { get; set; }
        public int MaxEmployees { get; set; }
    }
}