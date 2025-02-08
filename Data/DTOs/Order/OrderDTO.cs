


namespace Data.DTOs.Order
{
    public class OrderDTO
    {   
        public string OrderCode { get; set; }
        public double TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsShared { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime PaidDate { get; set; }
        public int? CustomerId { get; set; }
        public int PartnerId { get; set; }
        public int? ContactId { get; set; }
        public List<EmployeeAccessDTO> EmployeeAccessLevels { get; set; } = new();
    }
}