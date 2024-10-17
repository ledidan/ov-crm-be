namespace Data.Entities
{
    public class ProductCatelogy
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public virtual Partner? Partner { get; set; }
    }
}
