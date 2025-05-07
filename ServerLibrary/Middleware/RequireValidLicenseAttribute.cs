


using System.Text.Json;
using Data.Applications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using ServerLibrary.Services;

namespace ServerLibrary.MiddleWare
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireValidLicenseAttribute : TypeFilterAttribute
    {
        public RequireValidLicenseAttribute() : base(typeof(RequireValidLicenseFilter))
        {
        }

        private class RequireValidLicenseFilter : IAsyncAuthorizationFilter
        {
            private readonly ILicenseCenterService _licenseService;
            private readonly ILogger<RequireValidLicenseFilter> _logger;
            public RequireValidLicenseFilter(ILicenseCenterService licenseService, ILogger<RequireValidLicenseFilter> logger)
            {
                _licenseService = licenseService;
                _logger = logger;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                var partnerIdClaim = context.HttpContext.User.FindFirst("PartnerId");
                if (partnerIdClaim == null || !int.TryParse(partnerIdClaim.Value, out int partnerId))
                {
                    context.Result = new UnauthorizedObjectResult("Missing or invalid PartnerId");
                    return;
                }

                var appsClaim = context.HttpContext.User.FindFirst("Apps")?.Value;
                if (string.IsNullOrEmpty(appsClaim))
                {
                    context.Result = new UnauthorizedObjectResult("Missing Apps claim");
                    return;
                }

                List<AppClaim>? apps;
                try
                {
                    apps = JsonSerializer.Deserialize<List<AppClaim>>(appsClaim);
                    if (apps == null || !apps.Any())
                    {
                        context.Result = new UnauthorizedObjectResult("No valid apps found in Apps claim");
                        return;
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing Apps claim: {ex.Message}");
                    context.Result = new UnauthorizedObjectResult("Invalid Apps claim format");
                    return;
                }

                string? appIdStr = null;

                appIdStr = context.HttpContext.Request.Query["appId"].ToString();
                if (string.IsNullOrEmpty(appIdStr))
                    appIdStr = context.HttpContext.Request.Query["AppId"].ToString();

                if (string.IsNullOrEmpty(appIdStr))
                    appIdStr = context.HttpContext.Request.RouteValues["appId"]?.ToString();

                if (string.IsNullOrEmpty(appIdStr))
                    appIdStr = context.HttpContext.Request.Headers["X-App-Id"].ToString();

                if (string.IsNullOrEmpty(appIdStr) && 
                    (context.HttpContext.Request.Method == "POST" || context.HttpContext.Request.Method == "PUT"))
                {
                    try
                    {
                        context.HttpContext.Request.EnableBuffering();
                        using var reader = new StreamReader(
                            context.HttpContext.Request.Body,
                            encoding: System.Text.Encoding.UTF8,
                            detectEncodingFromByteOrderMarks: false,
                            leaveOpen: true);
                        var body = await reader.ReadToEndAsync();
                        context.HttpContext.Request.Body.Position = 0;

                        if (!string.IsNullOrEmpty(body))
                        {
                            var json = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
                            appIdStr = json?.GetValueOrDefault("appId")?.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading body: {ex.Message}");
                    }
                }
                if (string.IsNullOrEmpty(appIdStr))
                {
                    var hasValidApp = false;
                    foreach (var app in apps)
                    {
                        if (int.TryParse(app.AppId, out int appId) && 
                            await _licenseService.ValidLicenseAsync(appId, context.HttpContext.User))
                        {
                            hasValidApp = true;
                            break;
                        }
                    }
                    if (!hasValidApp)
                    {
                        context.Result = new ObjectResult("No valid licenses found in Apps")
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return;
                    }
                }
                else
                {
                    if (!int.TryParse(appIdStr, out int applicationId))
                    {
                        context.Result = new UnauthorizedObjectResult("Invalid ApplicationId format");
                        return;
                    }

                    var isValid = await _licenseService.ValidLicenseAsync(applicationId, context.HttpContext.User);
                    if (!isValid)
                    {
                        context.Result = new ObjectResult("Invalid or expired license for AppId")
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return;
                    }
                }
            }
        }
    }
}