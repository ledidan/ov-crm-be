
using System.ComponentModel.DataAnnotations;
using Data.Enums;
using Data.MongoModels;

namespace Data.DTOs
{
    public class QuoteDTO
    {
        public int Id { get; set; }

        public string? QuoteNo { get; set; }

        public DateTime? QuoteDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public int? CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public int? ContactId { get; set; }

        public string? ContactName { get; set; }

        public decimal TotalSummary { get; set; }

        public string? StageID { get; set; }

        public string? Description { get; set; }

        public decimal ToCurrencySummary { get; set; }

        public decimal DiscountSummary { get; set; }

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


        [Required(ErrorMessage = "Thông tin hàng hoá không được để trống")]
        public required List<QuoteDetailsDTO> QuoteDetails { get; set; } = new List<QuoteDetailsDTO>();
    }

}