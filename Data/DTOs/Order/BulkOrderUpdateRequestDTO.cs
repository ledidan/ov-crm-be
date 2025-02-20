


namespace Data.DTOs
{
    public class BulkOrderUpdateRequest
    {
        public List<int> OrderIds { get; set; } 
        public int? ContactId { get; set; } 
        public int? CustomerId { get; set; } 
        public int? EmployeeId { get; set; } 
    }

}