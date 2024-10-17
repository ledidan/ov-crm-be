using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public async Task<T> AddToDatabase<T>(T model)
        {
            var result = this.Add(model!);
            await this.SaveChangesAsync();
            return (T)result.Entity;
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<SystemRole> SystemRoles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshTokenInfo> RefreshTokenInfos { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<PartnerUser> PartnerUsers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCatelogy> ProductCatelogies { get; set; }
        public DbSet<ProductInventory> ProductInventories { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
    }
}
