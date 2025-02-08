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
        (
        IPartnerService partnerService,
        IProductService productService,
        IEmployeeService employeeService) : ControllerBase
    {
        [HttpGet("get-all")]
        [Authorize(Roles = "User,Admin")]
        public async Task<List<ProductDTO>> GetProductsAsync()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee  = await employeeService.FindByClaim(identity);
            if (partner == null) return new List<ProductDTO>();

            var result = await productService.GetAllAsync(employee, partner);
            return result;
        }

        [HttpPost("create")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateProductAsync(CreateProduct product)
        {
            if (product == null) return BadRequest("Model is empty");

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null) return BadRequest("Partner not found");

            var result = await productService.CreateAsync(product, employee, partner);
            return Ok(result);
        }

        // [HttpPost("update-product")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> UpdateProductAsync(Product product)
        // {
        //     if (product == null) return BadRequest("Model is empty");

        //     var result = await productService.UpdateAsync(product);
        //     return Ok(result);
        // }

        // [HttpPost("update-sellingprice")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> UpdateSellingPriceAsync(Product product, double sellingPrice)
        // {
        //     if (product == null) return BadRequest("Model is empty");

        //     var result = await productService.UpdateSellingPriceAsync(product, sellingPrice);
        //     return Ok(result);
        // }
    }
}
