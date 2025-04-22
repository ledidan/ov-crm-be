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
    public class ProductCategoryController : ControllerBase
    {
        private readonly IPartnerService _partnerService;
        private readonly IProductCategoryService _productCategoryService;

        private readonly IEmployeeService _employeeService;
        public ProductCategoryController(IProductCategoryService productCategoryService, IPartnerService partnerService, IEmployeeService employeeService)
        {
            _partnerService = partnerService;
            _productCategoryService = productCategoryService;
            _employeeService = employeeService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateProductCategoryAsync(CreateProductCategory productCategory)
        {
            if (productCategory == null) return BadRequest("Model is empty");

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null) return BadRequest("Partner not found");

            var result = await _productCategoryService.CreateAsync(productCategory, employee, partner);
            return Ok(result);
        }

        [HttpGet("productCategories")]
        [Authorize(Roles = "User,Admin")]
        public async Task<List<AllProductCategoryDTO>> GetProductCategoriesAsync()
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (partner == null || employee == null)
            {
                return new List<AllProductCategoryDTO>();
            }
            var result = await _productCategoryService.GetAllAsync(employee, partner);
            return result;
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetProductCategoryById(int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (employee == null) return BadRequest("employee null");
            if (partner == null) return BadRequest("partner null");

            var result = await _productCategoryService.FindById(id, employee, partner);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateProductCategoryDTO productCategory)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null) return BadRequest("Unauthorized Partner");
            if (partner == null)
            {
                return BadRequest("Invalid partner.");
            }

            var result = await _productCategoryService.UpdateAsync(id, productCategory, partner, employee);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("bulk-delete")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> RemoveCategories([FromQuery] string ids)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            if (partner == null) return BadRequest("Unauthorized Partner");
            if (string.IsNullOrWhiteSpace(ids))
            {
                return BadRequest("Invalid request. No category IDs provided.");
            }
            var result = await _productCategoryService.RemoveBulkIdsAsync(ids, partner);
            return Ok(result);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateProductCategory(int id, [FromBody] UpdateProductCategoryDTO productCategory)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);

            if (productCategory == null)
                return BadRequest(new { message = "Invalid request data" });

            if (employee == null || partner == null)
                return Unauthorized(new { message = "Unauthorized access" });

            var result = await _productCategoryService.UpdateFieldIdAsync(id, productCategory, employee, partner);

            if (result == null || !result.Flag)
                return NotFound(new { message = result?.Message ?? "Product category not found" });

            return Ok(new { Flag = result.Flag, Message = result.Message });
        }
        [HttpPost("check-code")]
        public async Task<IActionResult> CheckProductCategoryCode([FromBody] string code)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _productCategoryService.CheckProductCategoryCodeAsync(code, employee, partner);

            if (!result.Flag)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateProductCategoryCode()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var partner = await _partnerService.FindByClaim(identity);
            var employee = await _employeeService.FindByClaim(identity);
            if (partner == null || employee == null)
                return BadRequest("Model is empty");

            var result = await _productCategoryService.GenerateProductCategoryCodeAsync(partner);

            if (!result.Flag)
                return BadRequest(result.Message);

            return Ok(result);
        }
    }
}