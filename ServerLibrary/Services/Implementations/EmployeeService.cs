using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Enums;
using Data.Responses;
using Mapper.EmployeeMapper;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class EmployeeService(AppDbContext appDbContext,
        IPartnerService partnerService) : IEmployeeService
    {
        public async Task<EmployeeDTO> CreateEmployeeAdminAsync(CreateEmployee employee)
        {
            //check partner
            var partner = await partnerService.FindById(employee.PartnerId);
            if (partner == null) throw new KeyNotFoundException("Partner not found");

            var checkEmployeeExist = await CheckMatchingEmployeeCode(employee.EmployeeCode, partner.Id);
            if (checkEmployeeExist == true)
            {
                throw new ArgumentException("Mã nhân viên đã tồn tại, vui lòng nhập mã khác");
            }
            var newEmployee = new Employee()
            {
                EmployeeCode = employee.EmployeeCode,
                FullName = employee.FullName,
                PhoneNumber = employee.PhoneNumber,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth ?? null,
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
                CRMRoleId = employee.CRMRoleId,
                Partner = partner
            };
            await appDbContext.InsertIntoDb(newEmployee);

            return newEmployee.ToEmployeeDTO();
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
        public async Task<PagedResponse<List<EmployeeDTO>>> GetAllAsync(Partner partner, int pageNumber, int pageSize)
        {
            if (partner == null)
            {
                throw new ArgumentNullException(nameof(partner), "Partner null là không được");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var query = appDbContext.Employees
                .Where(e => e.PartnerId == partner.Id)
                .Include(e => e.Contacts);

            var totalRecords = await query.CountAsync();

            var employees = await query
                .OrderBy(e => e.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<List<EmployeeDTO>>(
                data: employees.Select(x => x.ToAllEmployeeDTO()).ToList(),
                pageNumber: pageNumber,
                pageSize: pageSize,
                totalRecords: totalRecords
            );
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

        public async Task<DataStringResponse> CreateEmployeeAsync(CreateEmployee employee, Partner partner)
        {
            if (employee == null) return new DataStringResponse(false, "Không tìm thấy nhân viên", null);

            //check partner
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
                DateOfBirth = employee.DateOfBirth ?? new DateTime(),
                JobTitleGroupId = employee.JobTitleGroupId,
                JobPositionGroupId = employee.JobPositionGroupId,
                Email = employee.Email,
                Address = employee.Address,
                OfficePhone = employee.OfficePhone,
                OfficeEmail = employee.OfficeEmail,
                TaxIdentificationNumber = employee.TaxIdentificationNumber,
                SignedContractDate = employee.SignedContractDate ?? null,
                SignedProbationaryContract = employee.SignedProbationaryContract,
                Resignation = employee.Resignation,
                JobStatus = JobStatus.Active,
                CRMRoleId = employee.CRMRoleId,
                Partner = partner
            };
            await appDbContext.InsertIntoDb(newEmployee);

            return new DataStringResponse(true, "Khởi tạo hồ sơ nhân viên thành công !", newEmployee.Id.ToString());
        }

    }
}
