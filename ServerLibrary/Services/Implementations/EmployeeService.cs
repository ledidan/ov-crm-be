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

            await appDbContext.AddToDatabase(new Employee() {
                CivilId = employee.CivilId,
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

        public async Task<List<Employee>> GetAllAsync(int partnerId)
        {
            //check partner
            var partner = await partnerService.FindById(partnerId);
            if (partner == null) return new List<Employee>();

            var result = await appDbContext.Employees.Where(_ => _.Partner.Id == partnerId).ToListAsync();
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
    }
}
