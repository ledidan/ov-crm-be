using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Data.DTOs;
using Data.Entities;
using Data.Interceptor;

namespace Data.DTOs
{
    public class OrderInvoiceDTO
    {
        public int Id { get; set; }
        // [Required]
        public required string SaleOrderNo { get; set; } = string.Empty;

        [Required]
        public decimal SaleOrderAmount { get; set; }

        public string? SaleOrderName { get; set; }
        [JsonConverter(typeof(NullableBoolConverter))]
        public bool? IsPaid { get; set; } = false;
        [JsonConverter(typeof(NullableBoolConverter))]
        public bool? IsShared { get; set; } = false;
        public string? SaleOrderTypeID { get; set; }
        public string? Description { get; set; }
        public string? StatusID { get; set; }
        public string? RevenueStatusID { get; set; }
        public string? RecordedSale { get; set; }

        public DateTime SaleOrderDate { get; set; }

        [Required]
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? DueDate { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? DeadlineDate { get; set; }

        [Required]
        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? BookDate { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? PaidDate { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? InvoiceDate { get; set; }

        [JsonConverter(typeof(NullableDateTimeConverter))]
        public DateTime? RevenueRecognitionDate { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? TotalReceiptedAmount { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? BalanceReceiptAmount { get; set; }
        [JsonConverter(typeof(NullableBoolConverter))]
        public bool? IsInvoiced { get; set; } = false;

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? InvoicedAmount { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? UnInvoicedAmount { get; set; }
        public string? TaxCode { get; set; }
        public string? ShippingReceivingPerson { get; set; }
        public string? InvoiceReceivingEmail { get; set; }
        public string? InvoiceReceivingPhone { get; set; }

        public string? BillingContactID { get; set; }
        public string? BillingCountryID { get; set; }
        public string? BillingProvinceID { get; set; }
        public string? BillingDistrictID { get; set; }
        public string? BillingWardID { get; set; }
        public string? BillingStreet { get; set; }
        public string? BillingCode { get; set; }

        public string? Phone { get; set; }

        public string? ShippingCountryID { get; set; }
        public string? ShippingProvinceID { get; set; }
        public string? ShippingDistrictID { get; set; }
        public string? ShippingWardID { get; set; }
        public string? ShippingStreet { get; set; }
        public string? ShippingCode { get; set; }
        [JsonConverter(typeof(NullableIntConverter))]
        public int? ModifiedBy { get; set; }
        [JsonConverter(typeof(NullableBoolConverter))]
        public bool? IsPublic { get; set; } = false;
        [JsonConverter(typeof(NullableBoolConverter))]
        public bool? IsDeleted { get; set; } = false;

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? TotalSummary { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? DiscountAfterTaxSummary { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? DiscountSummary { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? ToCurrencySummary { get; set; }

        [JsonConverter(typeof(NullableIntConverter))]
        public int? NumberOfDaysOwed { get; set; }
        [JsonConverter(typeof(NullableIntConverter))]
        public int? BillingAccountID { get; set; }
        public string? BillingAccountIDText { get; set; }
        [JsonConverter(typeof(NullableBoolConverter))]
        public bool? IsSentBill { get; set; } = false;
        [JsonConverter(typeof(NullableBoolConverter))]
        public bool? IsContractPartner { get; set; } = false;
        public string? DeliveryStatusID { get; set; }
        public string? ShippingContactID { get; set; }
        public string? PayStatusID { get; set; }
        public string? PayStatusIDText { get; set; }
        public string? ContractNumber { get; set; }
        [JsonConverter(typeof(NullDoubleConverter))]
        public double? SaleOrderProcessCost { get; set; }

        public string? RecordedSaleUsersID { get; set; }
        public string? RecordedSaleOrganizationUnitID { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? LiquidateAmount { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? NotPaidAmountSummary { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? PaidAmountSummary { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? RemainingAmount { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? ReturnedSummary { get; set; }

        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? RevenueAccountingAmount { get; set; }
        [JsonConverter(typeof(NullableIntConverter))]
        public int? CustomerId { get; set; }
        [JsonConverter(typeof(NullableIntConverter))]
        public int? OwnerTaskExecuteId { get; set; }
    }
}
