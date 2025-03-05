


using System.Security.Claims;
using AutoMapper;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{

    public class AccountService : IAccountService
    {

        private readonly AppDbContext _appDbContext;

        public AccountService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<GeneralResponse> DeactivateAccount(int userId, Partner partner)
        {
            var user = await _appDbContext.ApplicationUsers
              .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new GeneralResponse(false, "Không tìm thấy người dùng.");
            if (!user.IsActive.HasValue || !user.IsActive.Value)
                return new GeneralResponse(false, "Người dùng đã bị vô hiệu hóa.");

            user.IsActive = false;
            _appDbContext.Update(user);
            await _appDbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Người dùng đã vô hiệu hóa thành công.");
        }

        public async Task<bool?> FindByClaim(ClaimsIdentity? claimsIdentity)
        {
            var value = claimsIdentity?.FindFirst("Owner")?.Value;
            if (value == null) return false;
            if (value == "true")
            {
                return true;
            }
            return false;
        }

    }

}