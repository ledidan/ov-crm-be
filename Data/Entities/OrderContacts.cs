using Data.Enums;

namespace Data.Entities
{
    public class OrderContacts
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ContactId { get; set; }
        public Contact Contact { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }
    }
}
