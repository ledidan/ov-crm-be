



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IPartnerService _partnerService;

        public AdminController(IEmployeeService employeeService, IPartnerService partnerService)
        {
            _employeeService = employeeService;
            _partnerService = partnerService;
        }

    }
}