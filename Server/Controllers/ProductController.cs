using Data.DTOs;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.MiddleWare;
using ServerLibrary.Services.Interfaces;
using System.Security.Claims;

namespace Server.Controllers
{
    [RequireValidLicense]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController
    (
    IPartnerService partnerService,
    IProductService productService,
    IEmployeeService employeeService) : ControllerBase
    {
        [HttpGet("products")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetProductsAsync([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null) return NotFound("Không tìm thấy đối tác");

            var result = await productService.GetAllAsync(employee, partner, pageNumber, pageSize);
            return Ok(result);
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
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null) return BadRequest("Partner not found");
            if (product == null) return BadRequest("Model is empty");

            var result = await productService.UpdateAsync(id, product, partner, employee);
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

        [HttpGet("{id:int}/orders")]
        public async Task<IActionResult> GetOrdersByProductIdAsync(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return BadRequest("Không tìm thấy đối tác");

            var result = await productService.GetOrdersByProductIdAsync(id, partner, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}/invoices")]
        public async Task<IActionResult> GetInvoicesByProductIdAsync(int id, [FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            if (partner == null) return BadRequest("Không tìm thấy đối tác");

            var result = await productService.GetInvoicesByProductIdAsync(id, partner, pageNumber, pageSize);
            return Ok(result);
        }


        [HttpPost("check-code")]
        public async Task<IActionResult> CheckProductCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await productService.CheckProductCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateContactCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await productService.GenerateProductCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpGet("productsInventory")]
        public async Task<IActionResult> GetProductsInventoryAsync([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await partnerService.FindByClaim(identity);
            var employee = await employeeService.FindByClaim(identity);
            if (partner == null) return NotFound("Không tìm thấy đối tác");

            var result = await productService.GetAllProductsWithInventoryAsync(employee, partner, pageNumber, pageSize);
            return Ok(result);
        }

    }
}
