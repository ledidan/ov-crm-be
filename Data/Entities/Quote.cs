


using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;

namespace Data.Entities
{
    public class Quote : BaseEntity
    {
        public int Id { get; set; }

        public string? QuoteNo { get; set; }

        public DateTime? QuoteDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public int? CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public int? ContactId { get; set; }

        public string? ContactName { get; set; }

        [Column(TypeName = "decimal(18,2)")]

        public decimal TotalSummary { get; set; }

        public string? StageID { get; set; }

        public string? Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]


        public decimal ToCurrencySummary { get; set; }
        [Column(TypeName = "decimal(18,2)")]


        public decimal DiscountSummary { get; set; }
        [Column(TypeName = "decimal(18,2)")]

        public decimal TaxSummary { get; set; }

        public int? OpportunityID { get; set; }

        public string? OpportunityNo { get; set; }

        public string? Address { get; set; }

        public string? AccountTel { get; set; }


        public string? SaleProjectID { get; set; }

        public string? OfficeEmail { get; set; }

        public string? Note { get; set; }

        public string? TaxCode { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public int? ApproverID { get; set; }

        public CurrencyType CurrencyTypeID { get; set; } = CurrencyType.VND;
        public int? OwnerTaskExecuteId { get; set; }
        public string? OwnerTaskExecuteName { get; set; }

        public int? RelatedUserId { get; set; }
        public int? PartnerId { get; set; }

        public List<Employee> Employees { get; set; } = new List<Employee>();
        public required Partner Partner { get; set; }
    }
}