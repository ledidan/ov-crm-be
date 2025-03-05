
namespace Data.Entities
{
    public class JobPositionGroup
    {
        public int Id { get; set; }

        public required string JobPositionGroupCode { get; set; }

        public required string JobPositionGroupName { get; set; }

        public int? PartnerId { get; set; }
        public Partner Partner { get; set; }
        
    }
}
