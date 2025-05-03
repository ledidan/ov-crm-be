using System.Security.Claims;
using Data.DTOs;
using Data.MongoModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services;
using ServerLibrary.Services.Interfaces;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SysAdminController(IUserService userService, IPartnerService partnerService) : ControllerBase
    {

        [HttpPost("create-sysadmin")]
        public async Task<IActionResult> CreateSysAdminAsync(RegisterSysAdmin user)
        {
            if (user == null) return BadRequest("User is empty");

            string role = Constants.Role.SysAdmin;
            var result = await userService.CreateSysAdminAsync(user, role);
            return Ok(result);
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = "SysAdmin")]
        public async Task<IActionResult> CreateAdminAsync(RegisterSysAdmin user)
        {
            string role = Constants.Role.Admin;
            var result = await userService.CreateSysAdminAsync(user, role);

            if (!result.Flag)
            {
                return BadRequest("Cannot create by user service create admin sys");
            }
            return Ok(result);
        }

        [HttpGet("test-s3")]
        public async Task<IActionResult> TestS3([FromServices] S3Service s3Service)
        {
            try
            {
                await s3Service.TestConnectionAsync();
                return Ok("S3 connected!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("test-mysql")]
        public async Task<IActionResult> TestMySql([FromServices] AppDbContext dbContext)
        {
            try
            {
                await dbContext.Database.CanConnectAsync();
                return Ok("MySQL connected!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("test-mongodb")]
        public async Task<IActionResult> TestMongoDb([FromServices] MongoDbContext mongoContext)
        {
            try
            {
                // Test connection by listing collections
                await mongoContext.OpportunityProductDetails.Database.ListCollectionNamesAsync();
                // Insert test document
                var collection = mongoContext.OpportunityProductDetails;
                await collection.InsertOneAsync(new OpportunityProductDetails {});
                return Ok("MongoDB connected!");
            }
            catch (MongoException ex)
            {
                return StatusCode(500, $"MongoDB connection failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }
    }
}