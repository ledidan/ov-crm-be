using Data.Enums;

namespace Data.Entities
{
    public class OpportunityContacts
    {
        public int OpportunityId { get; set; }
        public Opportunity Opportunity { get; set; }

        public int ContactId { get; set; }
        public Contact Contact { get; set; }

        public int PartnerId { get; set; }
        public Partner Partner { get; set; }
    }
}
