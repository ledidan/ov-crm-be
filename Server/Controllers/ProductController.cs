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
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null) return new List<ProductDTO>();

            var result = await productService.GetAllAsync(employee, partner);
            return result;
        }

        [HttpPost("create")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateProductAsync(CreateProductDTO product)
        {
            if (product == null) return BadRequest("Model is empty");

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null) return BadRequest("Partner not found");

            var result = await productService.CreateAsync(product, employee, partner);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetProductDetailAsync(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return BadRequest("Partner not found");

            var result = await productService.FindByIdAsync(id, partner);
            return Ok(result);
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] UpdateProductDTO product)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return BadRequest("Partner not found");
            if (product == null) return BadRequest("Model is empty");

            var result = await productService.UpdateAsync(id, product, partner);
            return Ok(result);
        }

        [HttpDelete("bulk-delete")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> RemoveProduct([FromQuery] string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return BadRequest("Unauthorized Partner");
            if (string.IsNullOrWhiteSpace(ids))
            {
                return BadRequest("Invalid request. No category IDs provided.");
            }
            var result = await productService.RemoveBulkIdsAsync(ids, partner);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDTO product)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);

            if (product == null)
                return BadRequest(new { message = "Invalid request data" });

            // Retrieve employee and partner (replace with actual logic)

            if (employee == null || partner == null)
                return Unauthorized(new { message = "Unauthorized access" });

            var result = await productService.UpdateFieldIdAsync(id, product, employee, partner);

            if (result == null || !result.Flag)
                return NotFound(new { message = result?.Message ?? "Product not found" });

            return Ok(new { Flag = result.Flag, Message = result.Message });
        }

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
