


namespace Data.DTOs
{
    public class LicenseForUserDTO
    {
        public int Id { get; set; }           // Primary Key}
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int? ApplicationPlanId { get; set; }
        public string PlanName { get; set; }
        public string LicenceType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? MaxEmployeesExpected { get; set; }
    }
}