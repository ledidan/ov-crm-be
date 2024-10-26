namespace Data.Entities
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public virtual required Order Order { get; set; }
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
        public double SellingPrice { get; set; }
        public double Amount { get; set; }
    }
}
