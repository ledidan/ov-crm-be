

namespace Data.Entities
{
    public class CompanyJobPosition : BaseEntity
    {
        public int Id { get; set; }

        public required string PositionCode { get; set; }

        public required string JobPositionName { get; set; }

        public int? JobPositionGroupId { get; set; }
        public int? JobTitleGroupId { get; set; }

        public bool? IsActive { get; set; }
        public required Partner Partner  { get; set; }
    }
}
