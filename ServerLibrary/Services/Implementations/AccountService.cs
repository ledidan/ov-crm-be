


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

        public async Task<List<TransactionForUserDTO>> GetAllHistoryPaymentLicenseAsync(int userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null");
            }
            var transactions = await _appDbContext.Transactions
                .Where(t => t.UserId == userId)
                .Select(t => new TransactionForUserDTO
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    ApplicationId = t.ApplicationId,
                    PaymentMethod = t.PaymentMethod,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                })
                .ToListAsync();
            if (transactions == null)
            {
                return new List<TransactionForUserDTO>();
            }
            return transactions;
        }

        public async Task<List<LicenseForUserDTO>> GetAllLicensesAsync(int userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null");
            }

            var licenses = _appDbContext.PartnerLicenses
                .Include(l => l.Application)
                .Include(p => p.ApplicationPlan)
                .Where(l => l.UserId == userId)
                .Select(l => new LicenseForUserDTO
                {
                    Id = l.Id,
                    MaxEmployeesExpected = l.MaxEmployeesExpected,
                    ApplicationId = l.ApplicationId,
                    ApplicationName = l.Application.Name,
                    PlanName = l.ApplicationPlan.Name,
                    LicenceType = l.LicenceType,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                })
                .ToList();
            return licenses;
        }

        public async Task<List<MergedEmployeeUserDTO>> GetMergedEmployeeUserDataAsync(Partner partner)
        {
            if (partner == null)
                throw new ArgumentNullException(nameof(partner), "Partner object cannot be null");

            var partnerId = partner.Id;

            var result = await _appDbContext.Employees
                .Where(e => e.PartnerId == partnerId) // Ensure partnerId exists
                .Join(
                    _appDbContext.PartnerUsers,
                    emp => emp.Id,
                    pu => pu.EmployeeId,
                    (emp, pu) => new { emp, pu }
                )
                .Join(
                    _appDbContext.ApplicationUsers,
                    emp_pu => emp_pu.pu.User.Id,
                    user => user.Id,
                    (emp_pu, user) => new MergedEmployeeUserDTO
                    {
                        EmployeeId = emp_pu.emp.Id,
                        EmployeeCode = emp_pu.emp.EmployeeCode,
                        EmployeeFullName = emp_pu.emp.FullName,
                        EmployeeGender = emp_pu.emp.Gender,
                        EmployeeDOB = emp_pu.emp.DateOfBirth,
                        EmployeePhone = emp_pu.emp.PhoneNumber,
                        EmployeeEmail = emp_pu.emp.Email,
                        EmployeeAddress = emp_pu.emp.Address,
                        OfficePhone = emp_pu.emp.OfficePhone,
                        OfficeEmail = emp_pu.emp.OfficeEmail,
                        TaxIdentificationNumber = emp_pu.emp.TaxIdentificationNumber,
                        JobStatus = emp_pu.emp.JobStatus,
                        SignedProbationaryContract = emp_pu.emp.SignedProbationaryContract,
                        Resignation = emp_pu.emp.Resignation,
                        SignedContractDate = emp_pu.emp.SignedContractDate,
                        PartnerId = emp_pu.emp.PartnerId,
                        UserId = user.Id,
                        // Avoid NullReferenceException by checking if `user` is not null
                        UserFullName = user.FullName ?? "N/A",
                        Avatar = user.Avatar ?? string.Empty,
                        UserEmail = user.Email ?? string.Empty,
                        UserPhone = user.Phone ?? string.Empty,
                        UserGender = user.Gender ?? string.Empty,
                        UserDOB = user.Birthday,
                        IsActive = user.IsActive ?? false,
                        AccountStatus = user.AccountStatus,
                        IsActivateEmail = user.IsActivateEmail ?? false
                    })
                .Skip(0).Take(50)
                .ToListAsync();
            return result;
        }


    }

}