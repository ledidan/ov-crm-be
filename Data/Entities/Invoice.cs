
using Data.Enums;

namespace Data.Entities
{
    public class Invoice : BaseEntity
    {
        public int Id { get; set; }
        public string? InvoiceRequestName { get; set; }
        public string? InvoiceAddress { get; set; }
        public string? Description { get; set; }
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
        public DateTime? RequestDate { get; set; }
        public decimal? AmountSummary { get; set; }
        public string? StatusID { get; set; }
        public string? TaxBudgetCode { get; set; }
        public bool? IsInvoicePaper { get; set; }
        public double? TaxSummary { get; set; }
        public double? DiscountSummary { get; set; }
        public decimal? ToCurrencyAfterDiscountSummary { get; set; }

        public decimal? ToCurrencySummary { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }

        public string? RecipientEmail { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerIdName { get; set; }
        public int? OwnerTaskExecuteId { get; set; }
        public string? OwnerTaskExecuteName { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public int? OrderId { get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public virtual required Partner Partner { get; set; }
        public ICollection<InvoiceEmployees> InvoiceEmployees { get; set; } = new List<InvoiceEmployees>();
        public ICollection<InvoiceOrders> InvoiceOrders { get; set; } = new List<InvoiceOrders>();

    }
}
