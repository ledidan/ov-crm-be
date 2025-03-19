using Data.Enums;

namespace Data.Entities
{
    public class InvoiceOrders
    {
        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

    }
}
