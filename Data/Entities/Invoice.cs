using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;

namespace Data.Entities
{
    public class Invoice : BaseEntity
    {
        public int Id { get; set; }

        public string? InvoiceRequestName { get; set; }
        public double? TotalSummary { get; set; }
        public PaymentType PaymentTypeId { get; set; }
        public string PaymentTypeIdText => PaymentTypeId.ToString();
        public CurrencyType CurrencyTypeId { get; set; }
        public string CurrencyTypeIdText => CurrencyTypeId.ToString();

        public InvoiceType InvoiceTypeId { get; set; }
        public string InvoiceTypeIdText => InvoiceTypeId.ToString();

        public DateTime? RequestDate { get; set; }
        public double? AmountSummary { get; set; }
        public double? TaxSummary { get; set; }
        public double? DiscountSummary { get; set; }
        public virtual required Customer Customer { get; set; }
        public Status? Status { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public ICollection<InvoiceEmployees> InvoiceEmployees { get; set; } = new List<InvoiceEmployees>();

    }
}
