using Data.Enums;

namespace Data.Entities
{
    public class CustomerOrders //** This table for linked to many orders with many customers
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

    }
}
