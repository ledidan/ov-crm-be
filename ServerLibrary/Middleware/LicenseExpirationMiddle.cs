



using Microsoft.AspNetCore.Http;
using ServerLibrary.Services;

namespace ServerLibrary.MiddleWare
{
    public class LicenseExpirationMiddleware
    {
        private readonly RequestDelegate _next;

        public LicenseExpirationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILicenseCenterService licenseService)
        {
            var partnerIdClaim = context.User.FindFirst("PartnerId");
            var appIdClaim = context.User.FindFirst("AppId"); 

            if (partnerIdClaim == null || appIdClaim == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing PartnerId or ApplicationId");
                return;
            }

            int partnerId = int.Parse(partnerIdClaim.Value);
            int applicationId = int.Parse(appIdClaim.Value);

            var isExpired = await licenseService.IsLicenseExpiredAsync(partnerId, applicationId);

            if (isExpired)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Your license has expired.");
                return;
            }

            await _next(context);
        }
    }

}