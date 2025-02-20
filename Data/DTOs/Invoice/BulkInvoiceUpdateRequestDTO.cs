


namespace Data.DTOs
{
    public class BulkInvoiceUpdateRequest
    {
        public List<int> InvoiceIds { get; set; } 
        public int? ContactId { get; set; } 
        public int? CustomerId { get; set; } 
        public int? EmployeeId { get; set; } 
    }

}