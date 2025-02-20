

using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
namespace Data.Interceptor
{
    public class TimestampInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context;

            if (context != null)
            {
                var timestamp = DateTime.UtcNow;

                foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.CreatedDate = timestamp; // Set CreatedAt
                        entry.Entity.ModifiedDate = timestamp; // Set ModifiedAt
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entry.Entity.ModifiedDate = timestamp; // Update ModifiedAt
                    }
                }
            }
            return base.SavingChanges(eventData, result);
        }
    }

}