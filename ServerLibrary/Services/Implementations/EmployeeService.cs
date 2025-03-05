using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class EmployeeService(AppDbContext appDbContext,
        IPartnerService partnerService) : IEmployeeService
    {
        public async Task<DataStringResponse> CreateAsync(CreateEmployee employee)
        {
            if (employee == null) return new DataStringResponse(false, "Không tìm thấy nhân viên", null);

            //check partner
            var partner = await partnerService.FindById(employee.PartnerId);
            if (partner == null) return new DataStringResponse(false, "Không tìm thấy tổ chức", null);

            var checkEmployeeExist = await CheckMatchingEmployeeCode(employee.EmployeeCode, partner.Id);
            if (checkEmployeeExist == true)
            {
                return new DataStringResponse(false, "Mã nhân viên đã tồn tại, vui lòng nhập mã khác");
            }
            var newEmployee = new Employee()
            {
                EmployeeCode = employee.EmployeeCode,
                FullName = employee.FullName,
                PhoneNumber = employee.PhoneNumber,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth,
                JobTitleGroupId = employee.JobTitleGroupId,
                JobPositionGroupId = employee.JobPositionGroupId,
                Email = employee.Email,
                Address = employee.Address,
                OfficePhone = employee.OfficePhone,
                OfficeEmail = employee.OfficeEmail,
                TaxIdentificationNumber = employee.TaxIdentificationNumber,
                SignedContractDate = employee.SignedContractDate,
                SignedProbationaryContract = employee.SignedProbationaryContract,
                Resignation = employee.Resignation,
                JobStatus = JobStatus.Active,
                Partner = partner
            };
            await appDbContext.InsertIntoDb(newEmployee);

            return new DataStringResponse(true, "Tạo nhân viên thành công", newEmployee.Id.ToString());
        }

        public async Task<Employee?> FindByIdAsync(int id)
        {
            var employee = await appDbContext.Employees.Include(x => x.Contacts)
          .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {id} not found.");
            }
            return employee;
        }
        public async Task<List<Employee>> GetAllAsync(int partnerId)
        {
            //check partner
            var partner = await partnerService.FindById(partnerId);
            if (partner == null) return new List<Employee>();

            var result = await appDbContext.Employees.Where(_ => _.PartnerId == partnerId).Include(c => c.Contacts).ToListAsync();
            return result;
        }

        public async Task<GeneralResponse> UpdateAsync(Employee employee)
        {
            if (employee == null) return new GeneralResponse(false, "Model is empty");

            //check employee
            var employeeUpdating = await appDbContext.Employees.FirstOrDefaultAsync(_ => _.Id == employee.Id);
            if (employeeUpdating == null) return new GeneralResponse(false, "Customer not found");

            appDbContext.Employees.Update(employee);
            appDbContext.SaveChanges();
            return new GeneralResponse(true, "Employee updated successfully");
        }


        public async Task<bool> EmployeeExists(int id, int partnerId)
        {
            return await appDbContext.Employees.AnyAsync(s => s.Id == id && s.PartnerId == partnerId);
        }

        private async Task<bool?> CheckMatchingEmployeeCode(string code, int? partnerId)
        {
            return await appDbContext.Employees.AnyAsync(e => e.EmployeeCode == code && e.PartnerId == partnerId);
        }

        public async Task<Employee?> FindByClaim(ClaimsIdentity? claimsIdentity)
        {
            try
            {
                var value = claimsIdentity?.FindFirst("EmployeeId")?.Value;
                if (value == null) return default(Employee);

                int employeeId = Int32.Parse(value);
                var employee = await FindByIdAsync(employeeId);
                return employee;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return default(Employee);
        }
    }
}
