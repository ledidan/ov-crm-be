using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Mapper.EmployeeMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IProductInventoryService _inventoryService;

        private readonly IPartnerService _partnerService;

        public InventoryController(
            IProductInventoryService inventoryService,
            IPartnerService partnerService
        )
        {
            _inventoryService = inventoryService;
            _partnerService = partnerService;
        }

        // GET: api/inventory
        [HttpGet]
        public async Task<IActionResult> GetAllInventories()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);

            if (partner == null)
            {
                return BadRequest("Đối tác không tồn tại");
            }

            var response = await _inventoryService.GetAllInventoriesAsync(partner);
            return Ok(response);
        }

        // GET: api/inventory/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventory(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);

            if (partner == null)
            {
                return BadRequest("Đối tác không tồn tại");
            }

            var response = await _inventoryService.GetInventoryByIdAsync(id, partner);
            if (!response.Flag)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        // POST: api/inventory
        [HttpPost]
        public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryDTO inventory)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if (!ModelState.IsValid)
            {
                return BadRequest("Dữ liệu đầu vào không hợp lệ");
            }
            if (partner == null)
            {
                return BadRequest("Đối tác không tồn tại");
            }

            var response = await _inventoryService.CreateInventoryAsync(inventory, partner);
            if (!response.Flag)
            {
                return BadRequest(response);
            }
            return CreatedAtAction(
                nameof(GetInventory),
                new { id = ((InventoryDTO)response.Data).Id },
                response
            );
        }

        // PUT: api/inventory/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(
            int id,
            [FromBody] UpdateInventoryDTO inventory
        )
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if (id != inventory.Id)
            {
                return BadRequest("ID không khớp");
            }

            if (partner == null)
            {
                return BadRequest("Đối tác không tồn tại");
            }

            var response = await _inventoryService.UpdateInventoryAsync(id, inventory, partner);
            if (!response.Flag)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        // GET: api/inventory/product/5
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetInventoriesByProductId(int productId)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if (partner == null)
            {
                return BadRequest("Đối tác không tồn tại");
            }

            var response = await _inventoryService.GetInventoriesByProductIdAsync(
                productId,
                partner
            );
            if (response == null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}
