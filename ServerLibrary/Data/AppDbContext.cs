using Data.Entities;
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
            builder.Entity<SystemRole>().HasData(
                 new SystemRole { Id = 1, Name = Constants.Role.User },
                 new SystemRole { Id = 2, Name = Constants.Role.Admin },
                 new SystemRole { Id = 3, Name = Constants.Role.SysAdmin }
             );
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
            base.OnModelCreating(builder);

        }
        public DbSet<InvoiceEmployees> InvoiceEmployees { get; set; }
        public DbSet<CustomerEmployees> CustomerEmployees { get; set; }
        public DbSet<ProductEmployees> ProductEmployees { get; set; }
        public DbSet<ContactEmployees> ContactEmployees { get; set; }
        public DbSet<OrderEmployees> OrderEmployees { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<SystemRole> SystemRoles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshTokenInfo> RefreshTokenInfos { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<PartnerUser> PartnerUsers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<Activity> Activities { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ProductInventory> ProductInventories { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
    }
}
