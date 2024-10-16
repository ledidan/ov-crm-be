namespace Data.Entities
{
    public class ProductPrice
    {
        public int Id { get; set; }
        public virtual required Product Product { get; set; }
        public double Price { get; set; }
        public bool IsLatest { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
