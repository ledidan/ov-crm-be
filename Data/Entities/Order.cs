namespace Data.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public double TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime PaidDate { get; set; }
        public virtual required Customer Customer { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual Partner? Partner { get; set; }
    }
}
