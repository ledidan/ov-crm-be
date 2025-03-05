




using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ServerLibrary.Data;

namespace ServerLibrary.Services
{

    public abstract class BaseService
    {
        protected readonly ClaimsPrincipal _user;
        protected readonly AppDbContext _appDbContext;
        protected readonly int _partnerId;

        protected bool IsOwner { get; set; }

        protected BaseService(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));

            _user = httpContextAccessor.HttpContext?.User
            ?? throw new ArgumentNullException(nameof(httpContextAccessor), "HttpContext is null");

            IsOwner = _user.Claims.Any(c => c.Type == "Owner" && c.Value == "true");

            _partnerId = int.TryParse(_user.Claims.FirstOrDefault(c => c.Type == "PartnerId")?.Value, out var id) ? id : 0;
        }

    }

}