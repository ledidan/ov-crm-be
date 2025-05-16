using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Interceptor;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Helpers;

namespace ServerLibrary.Data
{
    public class AppDbContext : DbContext
    {
        private readonly TimestampInterceptor _timestampInterceptor;

        public AppDbContext(DbContextOptions<AppDbContext> options, TimestampInterceptor timestampInterceptor) : base(options)
        {
            _timestampInterceptor = timestampInterceptor;
        }
        public AppDbContext(DbContextOptions<AppDbContext> options)
               : base(options)
        {
            _timestampInterceptor = new TimestampInterceptor();
        }

        // Generic method to insert a model into the database
        public async Task<T> InsertIntoDb<T>(T model) where T : BaseEntity
        {
            var timestamp = DateTime.UtcNow;
            // Set CreatedDate and ModifiedDate
            model.CreatedDate = timestamp;
            model.ModifiedDate = timestamp;

            var result = await this.AddAsync(model!);
            await this.SaveChangesAsync();
            return result.Entity;
        }

        // Generic method to update a model in the database/
        public async Task<T> UpdateDb<T>(T model) where T : BaseEntity
        {
            var timestamp = DateTime.UtcNow;

            // Update only the ModifiedDate
            model.ModifiedDate = timestamp;
            var result = this.Update(model!);
            await this.SaveChangesAsync();
            return result.Entity;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_timestampInterceptor != null)
            {
                optionsBuilder.AddInterceptors(_timestampInterceptor);
            }
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //** Generate Unique Code for Contact
            builder.Entity<Contact>()
    .HasIndex(c => new { c.ContactCode, c.PartnerId }) // Or just ContactCode if it's globally unique
    .IsUnique();

            //** Generate Unique Code for Customer
            builder.Entity<Customer>().HasIndex(c => new { c.AccountNumber, c.PartnerId }).IsUnique(); ; // Or just CustomerCode if it's globally unique
            //** Generate Unique Code for Product
            builder.Entity<Product>().HasIndex(c => new { c.ProductCode, c.PartnerId }).IsUnique(); ; // Or just ProductCode if it's globally unique
            //** Generate Unique Code for Order
            builder.Entity<Order>().HasIndex(c => new { c.SaleOrderNo, c.PartnerId }).IsUnique(); ; // Or just OrderCode if it's globally unique
            //** Generate Unique Code for Invoice
            builder.Entity<Invoice>().HasIndex(c => new { c.InvoiceRequestName, c.PartnerId }).IsUnique(); ; // Or just InvoiceCode if it's globally unique
            //** Generate Unique Code for Category
            builder.Entity<ProductCategory>().HasIndex(c => new { c.ProductCategoryCode, c.PartnerId }).IsUnique(); ; // Or just ActivityCode if it's globally unique
            builder.Entity<CustomerCare>().HasIndex(c => new { c.CustomerCareNumber, c.PartnerId }).IsUnique(); ;

            builder.Entity<SupportTicket>()
                .HasIndex(st => new { st.TicketNumber, st.PartnerId })
                .IsUnique();


            builder.Entity<ProductInventory>().HasIndex(pi => new { pi.InventoryCode, pi.PartnerId }).IsUnique();

            builder.Entity<Opportunity>()
                .HasIndex(o => new { o.OpportunityNo, o.PartnerId })
                .IsUnique();
            builder.Entity<Quote>()
                .HasIndex(q => new { q.QuoteNo, q.PartnerId })
                .IsUnique();

            builder.Entity<SystemRole>().HasData(
                 new SystemRole { Id = 1, Name = Constants.Role.User },
                 new SystemRole { Id = 2, Name = Constants.Role.Admin },
                 new SystemRole { Id = 3, Name = Constants.Role.SysAdmin }
             );
            // Seed CRMPermissions
            builder.Entity<CRMPermission>().HasData(SeedData.Permissions);
            builder.Entity<Application>().HasData(SeedData.Applications.ToArray());
            builder.Entity<ApplicationPlan>().HasData(SeedData.ApplicationPlans.ToArray());
            // builder.Entity<CRMRole>().HasData(SeedData.Roles.ToArray());
            builder.Entity<CRMRolePermission>()
           .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<PartnerLicense>(entity =>
   {
       entity.HasKey(pl => pl.Id);

       entity.HasOne(pl => pl.Partner)
           .WithMany(p => p.PartnerLicenses)
           .HasForeignKey(pl => pl.PartnerId)
           .IsRequired(false);

       entity.HasOne(pl => pl.Application)
           .WithMany(a => a.PartnerLicenses)
           .HasForeignKey(pl => pl.ApplicationId);

       entity.HasOne(pl => pl.ApplicationPlan)
           .WithMany(ap => ap.PartnerLicenses)
           .HasForeignKey(pl => pl.ApplicationPlanId)
           .OnDelete(DeleteBehavior.SetNull);
   });

            builder.Entity<ContactEmployees>()
        .Property(ce => ce.AccessLevel)
        .HasConversion<int>();
            builder.Entity<Contact>()
        .HasMany(c => c.Employees)
        .WithMany(e => e.Contacts)
        .UsingEntity<ContactEmployees>(
            j => j
                .HasOne(ce => ce.Employee)
                .WithMany(e => e.ContactEmployees)
                .HasForeignKey(ce => ce.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Contact)
                .WithMany(c => c.ContactEmployees)
                .HasForeignKey(ce => ce.ContactId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.ContactId, ce.EmployeeId, ce.PartnerId });
            });
            builder.Entity<CustomerEmployees>()
     .Property(ce => ce.AccessLevel)
     .HasConversion<int>();
            builder.Entity<Customer>()
        .HasMany(c => c.Employees)
        .WithMany(e => e.Customers)
        .UsingEntity<CustomerEmployees>(
            j => j
                .HasOne(ce => ce.Employee)
                .WithMany(e => e.CustomerEmployees)
                .HasForeignKey(ce => ce.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Customer)
                .WithMany(c => c.CustomerEmployees)
                .HasForeignKey(ce => ce.CustomerId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.CustomerId, ce.EmployeeId, ce.PartnerId });
            });
            builder.Entity<InvoiceEmployees>()
   .Property(ce => ce.AccessLevel)
   .HasConversion<int>();
            builder.Entity<Invoice>()
        .HasMany(c => c.Employees)
        .WithMany(e => e.Invoices)
        .UsingEntity<InvoiceEmployees>(
            j => j
                .HasOne(ce => ce.Employee)
                .WithMany(e => e.InvoiceEmployees)
                .HasForeignKey(ce => ce.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Invoice)
                .WithMany(c => c.InvoiceEmployees)
                .HasForeignKey(ce => ce.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.InvoiceId, ce.EmployeeId, ce.PartnerId });
            });

            builder.Entity<ActivityEmployees>()
   .Property(ce => ce.AccessLevel)
   .HasConversion<int>();
            builder.Entity<Activity>()
        .HasMany(c => c.Employees)
        .WithMany(e => e.Activities)
        .UsingEntity<ActivityEmployees>(
            j => j
                .HasOne(ce => ce.Employee)
                .WithMany(e => e.ActivityEmployees)
                .HasForeignKey(ce => ce.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Activity)
                .WithMany(c => c.ActivityEmployees)
                .HasForeignKey(ce => ce.ActivityId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.ActivityId, ce.EmployeeId, ce.PartnerId });
            });
            builder.Entity<ProductCategory>()
              .HasOne(pc => pc.ParentCategory)
              .WithMany(pc => pc.SubCategories)
              .HasForeignKey(pc => pc.ParentProductCategoryID)
              .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ProductEmployees>()
.Property(ce => ce.AccessLevel)
.HasConversion<int>();
            builder.Entity<Product>()
        .HasMany(c => c.Employees)
        .WithMany(e => e.Products)
        .UsingEntity<ProductEmployees>(
            j => j
                .HasOne(ce => ce.Employee)
                .WithMany(e => e.ProductEmployees)
                .HasForeignKey(ce => ce.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Product)
                .WithMany(c => c.ProductEmployees)
                .HasForeignKey(ce => ce.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.ProductId, ce.EmployeeId, ce.PartnerId });
            });
            builder.Entity<OrderEmployees>()
            .Property(ce => ce.AccessLevel)
            .HasConversion<int>();
            builder.Entity<Order>()
        .HasMany(c => c.Employees)
        .WithMany(e => e.Orders)
        .UsingEntity<OrderEmployees>(
            j => j
                .HasOne(ce => ce.Employee)
                .WithMany(e => e.OrderEmployees)
                .HasForeignKey(ce => ce.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Order)
                .WithMany(c => c.OrderEmployees)
                .HasForeignKey(ce => ce.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.OrderId, ce.EmployeeId, ce.PartnerId });
            });

            builder.Entity<Order>()
        .HasMany(c => c.Invoices)
        .WithMany(e => e.Orders)
        .UsingEntity<InvoiceOrders>(
            j => j
                .HasOne(ce => ce.Invoice)
                .WithMany(e => e.InvoiceOrders)
                .HasForeignKey(ce => ce.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Order)
                .WithMany(c => c.InvoiceOrders)
                .HasForeignKey(ce => ce.OrderId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.OrderId, ce.InvoiceId, ce.PartnerId });
            });
            builder.Entity<Order>()
        .HasMany(c => c.Contacts)
        .WithMany(e => e.Orders)
        .UsingEntity<OrderContacts>(
            j => j
                .HasOne(ce => ce.Contact)
                .WithMany(e => e.OrderContacts)
                .HasForeignKey(ce => ce.ContactId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Order)
                .WithMany(c => c.OrderContacts)
                .HasForeignKey(ce => ce.OrderId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.OrderId, ce.ContactId, ce.PartnerId });
            });

            builder.Entity<Customer>()
        .HasMany(c => c.Contacts)
        .WithMany(e => e.Customers)
        .UsingEntity<CustomerContacts>(
            j => j
                .HasOne(ce => ce.Contact)
                .WithMany(e => e.CustomerContacts)
                .HasForeignKey(ce => ce.ContactId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Customer)
                .WithMany(c => c.CustomerContacts)
                .HasForeignKey(ce => ce.CustomerId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.CustomerId, ce.ContactId, ce.PartnerId });
            });

            builder.Entity<Opportunity>()
        .HasMany(c => c.Contacts)
        .WithMany(e => e.Opportunities)
        .UsingEntity<OpportunityContacts>(
            j => j
                .HasOne(ce => ce.Contact)
                .WithMany(e => e.OpportunityContacts)
                .HasForeignKey(ce => ce.ContactId)
                .OnDelete(DeleteBehavior.Restrict),
            j => j
                .HasOne(ce => ce.Opportunity)
                .WithMany(c => c.OpportunityContacts)
                .HasForeignKey(ce => ce.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade),
            j =>
            {
                j.HasKey(ce => new { ce.OpportunityId, ce.ContactId, ce.PartnerId });
            });


            builder.Entity<Appointment>()
        .HasOne(a => a.Activity)
        .WithOne(a => a.Appointment)
        .HasForeignKey<Appointment>(a => a.ActivityId)
        .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Mission>()
                .HasOne(m => m.Activity)
                .WithOne(a => a.Mission)
                .HasForeignKey<Mission>(m => m.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Call>()
                .HasOne(c => c.Activity)
                .WithOne(a => a.Call)
                .HasForeignKey<Call>(c => c.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<ProductInventory>()
            .HasOne(i => i.Product)
            .WithMany(p => p.ProductInventories)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Inventory - Supplier
            builder.Entity<ProductInventory>()
                .HasOne(i => i.Supplier)
                .WithMany(s => s.ProductInventories)
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cấu hình mối quan hệ Product - Supplier
            builder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
            base.OnModelCreating(builder);
        }
        public DbSet<InvoiceEmployees> InvoiceEmployees { get; set; }
        public DbSet<CustomerContacts> CustomerContacts { get; set; }
        public DbSet<CustomerEmployees> CustomerEmployees { get; set; }
        public DbSet<ProductEmployees> ProductEmployees { get; set; }
        public DbSet<ContactEmployees> ContactEmployees { get; set; }
        public DbSet<OrderEmployees> OrderEmployees { get; set; }
        public DbSet<OpportunityContacts> OpportunityContacts { get; set; }
        public DbSet<InvoiceOrders> InvoiceOrders { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<OrderContacts> OrderContacts { get; set; }
        public DbSet<PasswordResetTokens> PasswordResetTokens { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<SystemRole> SystemRoles { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshTokenInfo> RefreshTokenInfos { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<PartnerUser> PartnerUsers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerCare> CustomerCares { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Mission> Missions { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductInventory> ProductInventories { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<CompanyJobPosition> CompanyJobPositions { get; set; }
        public DbSet<JobPositionGroup> JobPositionGroups { get; set; }
        public DbSet<JobTitleGroup> JobTitleGroups { get; set; }

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<Application> Applications { get; set; }
        public DbSet<PartnerLicense> PartnerLicenses { get; set; }

        public DbSet<ApplicationPlan> ApplicationPlans { get; set; }
        public DbSet<CRMRole> CRMRoles { get; set; }
        public DbSet<CRMPermission> CRMPermissions { get; set; }
        public DbSet<CRMRolePermission> CRMRolePermissions { get; set; }


        // ** Payment Dbset
        public DbSet<PaymentToken> PaymentTokens { get; set; }
        public DbSet<Transactions> Transactions { get; set; }

    }
}
