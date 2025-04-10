namespace Data.DTOs
{
    public class OrderStats
    {
        public int OrderCount { get; set; }
        public decimal? TotalOrderValue { get; set; }
        public decimal? TotalCollectedAmount { get; set; }
        public decimal? Debt { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public CustomerCycleDTO? PurchaseCycle { get; set; } // Chu kỳ mua hàng trung bình
        public List<OrderDetailDTO> PurchasedItems { get; set; } = new List<OrderDetailDTO>();
    }
}
