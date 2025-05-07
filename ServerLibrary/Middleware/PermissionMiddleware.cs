


using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;

namespace ServerLibrary.MiddleWare
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            var endpoint = context.GetEndpoint();
            var permissionAttributes = endpoint?.Metadata?.GetOrderedMetadata<HasPermissionAttribute>();

            if (permissionAttributes != null && permissionAttributes.Any())
            {
                var crmRoleIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "CRMRoleId")?.Value;

                if (crmRoleIdClaim == null || !int.TryParse(crmRoleIdClaim, out var crmRoleId))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("CRMRoleId missing or invalid.");
                    return;
                }

                var permissions = await dbContext.CRMRolePermissions
                    .Where(rp => rp.RoleId == crmRoleId)
                    .Select(rp => $"{rp.Permission.Action}:{rp.Permission.Resource}").ToListAsync();
       
                foreach (var attribute in permissionAttributes)
                {
                    var required = $"{attribute.Action}:{attribute.Resource}";
                    if (!permissions.Contains(required))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync($"Missing permission: {required}");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

}