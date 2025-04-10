using System.Security.Claims;
using System.Threading.Tasks;
using Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Implementations;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        private readonly IEmployeeService _employeeService;

        private readonly IPartnerService _partnerService;

        public DashboardController(
            IDashboardService dashboardService,
            IPartnerService partnerService,
            IEmployeeService employeeService
        )
        {
            _dashboardService = dashboardService;
            _partnerService = partnerService;
            _employeeService = employeeService;
        }

        [HttpGet("summaryOrders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderStatsForAllOrders()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            var stats = await _dashboardService.GetOrderStatsForAllOrders(employee, partner);
            return Ok(stats);
        }
        
    }
}
