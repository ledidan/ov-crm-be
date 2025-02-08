namespace Data.Entities
{
    public class ProductInventory : BaseEntity
    {
        public int Id { get; set; }
        public virtual required Product Product { get; set; }
        public double PurchasePrice { get; set; }
        public int TotalQuantity { get; set; }
        public int QuantitySold { get; set; }
    }
}
