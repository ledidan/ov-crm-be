using Data.DTOs;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController(IProductCatelogyService productCatelogyService,
        IPartnerService partnerService) : ControllerBase
    {
        /*[HttpGet("get-products")]
        public async Task<List<Product>> GetProductsAsync()
        {
            var result = await partnerService.GetAsync();
            return result;
        }*/

        [HttpPost("create-productcatelogy")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductCatelogyAsync(CreateProductCatelogy productCatelogy)
        {
            if (productCatelogy == null) return BadRequest("Model is empty");

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return BadRequest("Partner not found");

            var result = await productCatelogyService.CreateAsync(productCatelogy, partner);
            return Ok(result);
        }

        [HttpGet("get-productcatelogy")]
        [Authorize(Roles = "Admin")]
        public async Task<List<ProductCatelogy>> GetProductCatelogiesAsync()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return new List<ProductCatelogy>();

            var result = await productCatelogyService.GetAllAsync(partner);
            return result;
        }
    }
}
