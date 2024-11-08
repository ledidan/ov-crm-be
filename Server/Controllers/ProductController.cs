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
    public class ProductController
        (IProductCatelogyService productCatelogyService,
        IPartnerService partnerService,
        IProductService productService) : ControllerBase
    {
        [HttpGet("get-products")]
        public async Task<List<Product>> GetProductsAsync()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return new List<Product>();

            var result = await productService.GetAllAsync(partner);
            return result;
        }

        [HttpPost("create-product")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProductAsync(CreateProduct product)
        {
            if (product == null) return BadRequest("Model is empty");

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return BadRequest("Partner not found");

            var result = await productService.CreateAsync(product, partner);
            return Ok(result);
        }

        [HttpPost("update-product")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductAsync(Product product)
        {
            if (product == null) return BadRequest("Model is empty");

            var result = await productService.UpdateAsync(product);
            return Ok(result);
        }

        [HttpPost("update-sellingprice")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSellingPriceAsync(Product product, double sellingPrice)
        {
            if (product == null) return BadRequest("Model is empty");

            var result = await productService.UpdateSellingPriceAsync(product, sellingPrice);
            return Ok(result);
        }

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

        [HttpGet("get-productcatelogies")]
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
