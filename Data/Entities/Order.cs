


namespace Data.Entities
{
    public class Order : BaseEntity
    {
        public int Id { get; set; }
        public string? SaleOrderNo { get; set; }
        public decimal SaleOrderAmount { get; set; }

        public string? SaleOrderName { get; set; }
        public bool IsPaid { get; set; }
        public bool? IsShared { get; set; } = false;
        public string? SaleOrderTypeID { get; set; }

        public string? Description { get; set; }
        public string? StatusID { get; set; }
        public string? RevenueStatusID { get; set; }

        public string? RecordedSale { get; set; }
        public DateTime SaleOrderDate { get; set; }

        public DateTime? DueDate { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public DateTime? BookDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public decimal? TotalReceiptedAmount { get; set; }
        public decimal? BalanceReceiptAmount { get; set; }
        public bool IsInvoiced { get; set; }
        public decimal? InvoicedAmount { get; set; }
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
        public int? OwnerId { get; set; }
        public string? OwnerIdName { get; set; }
        public int? ModifiedBy { get; set; }

        public string? ModifiedByIdName { get; set; }
        public bool IsPublic { get; set; }
        public bool IsDeleted { get; set; }
        public decimal? TotalSummary { get; set; }
        public double? TaxSummary { get; set; }
        public decimal? DiscountAfterTaxSummary { get; set; }
        public decimal? DiscountSummary { get; set; }
        public decimal? ToCurrencySummary { get; set; }
        public int? NumberOfDaysOwed { get; set; }
        public int? BillingAccountID { get; set; }
        public string? BillingAccountIDText { get; set; }

        public bool? IsSentBill { get; set; }

        public bool? IsContractPartner { get; set; }

        public string? DeliveryStatusID { get; set; }

        public string? ShippingContactID { get; set; }

        public string? PayStatusID { get; set; }
        public string? PayStatusIDText { get; set; }

        public string? ContractNumber { get; set; }

        public double SaleOrderProcessCost { get; set; }

        public string? RecordedSaleUsersID { get; set; }

        public string? RecordedSaleOrganizationUnitID { get; set; }

        public decimal? LiquidateAmount { get; set; }
        public decimal? NotPaidAmountSummary { get; set; }
        public decimal? PaidAmountSummary { get; set; }
        public decimal? RemainingAmount { get; set; }

        public decimal? ReturnedSummary { get; set; }

        public decimal? RevenueAccountingAmount { get; set; }
        public DateTime? RevenueRecognitionDate { get; set; }
        public int? CustomerId { get; set; }    
        public string? CustomerName { get; set; }
        public int? OwnerTaskExecuteId { get; set; }
        public string? OwnerTaskExecuteName { get; set; }
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public List<Contact> Contacts { get; set; } = new List<Contact>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<Invoice> Invoices   { get; set; } = new List<Invoice>();
        public required Partner Partner { get; set; }
        public ICollection<OrderEmployees> OrderEmployees { get; set; } = new List<OrderEmployees>();
        public ICollection<OrderContacts> OrderContacts { get; set; } = new List<OrderContacts>();
        public ICollection<CustomerOrders> CustomerOrders { get; set; } = new List<CustomerOrders>();

           public ICollection<InvoiceOrders> InvoiceOrders { get; set; } = new List<InvoiceOrders>();

    }
}