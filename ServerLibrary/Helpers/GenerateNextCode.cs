



using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ServerLibrary.Helpers
{
    public class GenerateNextCode
    {
        private readonly DbContext _dbContext;

        public GenerateNextCode(DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<string> GenerateNextCodeAsync<T>(string prefix, Expression<Func<T, string>> codeSelector) where T : class
        {
            var dbSet = _dbContext.Set<T>();
            
            var latestEntity = await dbSet
        .OrderByDescending(codeSelector)
        .FirstOrDefaultAsync();

            if (latestEntity == null)
            {
                return $"{prefix}0000001"; // Start from 0000001 if no records exist
            }

            // Extract the code value dynamically
            var propertyInfo = (codeSelector.Body as MemberExpression)?.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new InvalidOperationException("Invalid code selector expression.");
            }

            string latestCode = propertyInfo.GetValue(latestEntity)?.ToString();

            if (string.IsNullOrEmpty(latestCode) || !latestCode.StartsWith(prefix))
            {
                return $"{prefix}0000001";
            }

            // Extract numeric part
            string numberPart = latestCode.Substring(prefix.Length); // Remove prefix (e.g., "LH" -> "0000123")

            if (int.TryParse(numberPart, out int numericCode))
            {
                numericCode++; // Increment the number
                return $"{prefix}{numericCode:D7}"; // Format as LH0000006
            }

            return $"{prefix}0000001"; // Fallback in case of an error 
        }
    }
}