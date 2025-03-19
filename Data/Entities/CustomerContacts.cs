using Data.Enums;

namespace Data.Entities
{
    public class CustomerContacts
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int ContactId { get; set; }
        public Contact Contact { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }

    }
}
