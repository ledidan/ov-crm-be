



using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IPartnerService _partnerService;

        public AccountController(IAccountService accountService, IPartnerService partnerService)
        {
            _accountService = accountService;
            _partnerService = partnerService;
        }

    }
}