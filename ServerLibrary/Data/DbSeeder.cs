

using Data.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ServerLibrary.Data
{
    public static class DbSeeder
    {
        public static void Seed(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();

            if (!db.CRMRoles.Any()) db.CRMRoles.AddRange(SeedData.Roles);
            if (!db.CRMPermissions.Any()) db.CRMPermissions.AddRange(SeedData.Permissions);
            if (!db.CRMRolePermissions.Any()) db.CRMRolePermissions.AddRange(SeedData.RolePermissions);

            db.SaveChanges();
        }
    }

}