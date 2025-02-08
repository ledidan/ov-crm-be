using System.Security.Claims;
using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class EmployeeService(AppDbContext appDbContext,
        IPartnerService partnerService) : IEmployeeService
    {
        public async Task<GeneralResponse> CreateAsync(CreateEmployee employee)
        {
            if (employee == null) return new GeneralResponse(false, "Model is empty");

            //check partner
            var partner = await partnerService.FindById(employee.PartnerId);
            if (partner == null) return new GeneralResponse(false, "Partner not found");

            await appDbContext.InsertIntoDb(new Employee()
            {
                Fullname = employee.Fullname,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth,
                PhoneNumber = employee.PhoneNumber,
                JobTitle = employee.JobTitle,
                Email = employee.Email,
                StreetAddress = employee.StreetAddress,
                District = employee.District,
                Province = employee.Province,
                TaxIdentificationNumber = employee.TaxIdentificationNumber,
                SignedContractDate = employee.SignedContractDate,
                Partner = partner,
            });

            return new GeneralResponse(true, "Employee created");
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

            var result = await appDbContext.Employees.Where(_ => _.Partner.Id == partnerId).Include(c => c.Contacts).ToListAsync();
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

        public async Task<bool> EmployeeExists(int id)
        {
            return await appDbContext.Employees.AnyAsync(s => s.Id == id);
        }
        public Task<List<ContactEmployees>> GetAllContactEmployees()
        {
            throw new NotImplementedException();
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
