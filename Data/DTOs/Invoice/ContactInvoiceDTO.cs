using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Data.Entities;
using Data.Enums;
using Data.Interceptor;

namespace Data.DTOs
{
    public class ContactInvoiceDTO
    {
        public int Id { get; set; }
        public string? InvoiceRequestName { get; set; }
        public string? InvoiceAddress { get; set; }
        public string? Description { get; set; }
        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? TotalSummary { get; set; }
        public string? PaymentTypeId { get; set; }
        public CurrencyType CurrencyTypeId { get; set; }
        public string? InvoiceTypeId { get; set; }
        public string? BankName { get; set; }
        public string? BillingCode { get; set; }
        public string? BillingCountryID { get; set; }
        public string? BillingDistrictID { get; set; }
        public string? BillingLat { get; set; }
        public string? BillingLong { get; set; }
        public string? BillingProvinceID { get; set; }
        public string? BankAccount { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? RequestDate { get; set; }
        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? AmountSummary { get; set; }
        public string? StatusID { get; set; }
        public string? TaxBudgetCode { get; set; }
        [JsonConverter(typeof(NullableBoolConverter))]
        public bool? IsInvoicePaper { get; set; }
        [JsonConverter(typeof(NullDoubleConverter))]
        public double? TaxSummary { get; set; }
        [JsonConverter(typeof(NullDoubleConverter))]
        public double? DiscountSummary { get; set; }
        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? ToCurrencyAfterDiscountSummary { get; set; }
        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? ToCurrencySummary { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? RecipientEmail { get; set; }
        public int OwnerId { get; set; }
        [JsonConverter(typeof(NullableIntConverter))]
        public int? BuyerId { get; set; }
        [JsonConverter(typeof(NullableIntConverter))]
        public int? CustomerId { get; set; }
        [JsonConverter(typeof(NullableIntConverter))]
        public int? EmployeeId { get; set; }
        public string? OwnerIdName { get; set; }
        public string? ModifiedByIdName { get; set; }
        public string? CustomerName { get; set; }
        public string? OwnerTaskExecuteName { get; set; }
        public string? BuyerName { get; set; }

        public int? OwnerTaskExecuteId { get; set; }
        public int PartnerId { get; set; }
        public List<int>? Orders { get; set; }
        [Required]
        public required List<InvoiceDetailDTO> InvoiceDetails { get; set; } = new List<InvoiceDetailDTO>();
    }
}
