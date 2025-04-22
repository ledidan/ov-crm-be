







using System.ComponentModel.DataAnnotations.Schema;
using Data.MongoModels;

namespace Data.Entities
{
    public class Opportunity : BaseEntity
    {
        public int Id { get; set; }

        public string? OpportunityNo { get; set; }
        public string? OpportunityName { get; set; }

        public string? Description { get; set; }
        public string? TagID { get; set; }

        public string? TagIDText { get; set; }

        public int? CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public int? ContactId { get; set; }

        public string? ContactName { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? ExpectedRevenue { get; set; }
        public DateTime? ClosingDate { get; set; }

        public string? ReasonWinLostID { get; set; }

        public string? ReasonWinLostIDText { get; set; }
        public string? StageID { get; set; }

        public string? StageIDText { get; set; }

        public float Probability { get; set; }

        public string? LeadSourceID { get; set; }

        public string? LeadSourceIDText { get; set; }

        public string? TypeID { get; set; }

        public string? TypeIDText { get; set; }

        public string? CountryID { get; set; }

        public string? ProvinceID { get; set; }

        public string? DistrictID { get; set; }

        public string? WardID { get; set; }

        public string? Street { get; set; }

        public bool? IsPublic { get; set; } = false;

        public int? OwnerTaskExecuteId { get; set; }
        public string? OwnerTaskExecuteName { get; set; }

        public int? PartnerId { get; set; }

        public List<Contact> Contacts { get; set; } = new List<Contact>();

        public List<Employee> Employees { get; set; } = new List<Employee>();

        public ICollection<OpportunityContacts> OpportunityContacts { get; set; } = new List<OpportunityContacts>();

        public virtual required Partner Partner { get; set; }
    }
}