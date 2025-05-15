



namespace Data.DTOs
{
    public class TransactionForUserDTO
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public int ApplicationId { get; set; }
        public int? ApplicationPlanId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // vnpay, momo
        public string Status { get; set; } // pending, success, failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}