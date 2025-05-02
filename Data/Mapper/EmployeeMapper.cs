
using Data.DTOs;
using Data.Entities;
using Mapper.ContactMapper;

namespace Mapper.EmployeeMapper
{
    public static class EmployeeMapper
    {
        public static EmployeeDTO ToEmployeeDTO(this Employee employeeModel)
        {
            return new EmployeeDTO
            {
                Id = employeeModel.Id,
                EmployeeCode = employeeModel.EmployeeCode,
                FullName = employeeModel.FullName,
                PhoneNumber = employeeModel.PhoneNumber,
                Gender = employeeModel.Gender,
                DateOfBirth = employeeModel.DateOfBirth ?? new DateTime(),
                Email = employeeModel.Email,
                Address = employeeModel.Address,
                OfficePhone = employeeModel.OfficePhone,
                OfficeEmail = employeeModel.OfficeEmail,
                TaxIdentificationNumber = employeeModel.TaxIdentificationNumber,
                JobPositionGroupId = employeeModel.JobPositionGroupId,
                JobTitleGroupId = employeeModel.JobTitleGroupId,
                JobStatus = employeeModel.JobStatus,
                SignedContractDate = employeeModel.SignedContractDate,
                SignedProbationaryContract = employeeModel.SignedProbationaryContract,
                Resignation = employeeModel.Resignation,
                PartnerId = employeeModel.PartnerId,
                ContactIds = employeeModel.Contacts.Select(c => c.Id).ToList(),
            };
        }
        public static EmployeeDTO ToAllEmployeeDTO(this Employee employeeModel)
        {
            return new EmployeeDTO
            {
                Id = employeeModel.Id,
                EmployeeCode = employeeModel.EmployeeCode,
                FullName = employeeModel.FullName,
                PhoneNumber = employeeModel.PhoneNumber,
                Gender = employeeModel.Gender,
                Address = employeeModel.Address,
                DateOfBirth = employeeModel.DateOfBirth ?? new DateTime(),
                Email = employeeModel.Email,
                OfficePhone = employeeModel.OfficePhone,
                OfficeEmail = employeeModel.OfficeEmail,
                TaxIdentificationNumber = employeeModel.TaxIdentificationNumber,
                JobPositionGroupId = employeeModel.JobPositionGroupId,
                JobTitleGroupId = employeeModel.JobTitleGroupId,
                JobStatus = employeeModel.JobStatus,
                SignedContractDate = employeeModel.SignedContractDate,
                SignedProbationaryContract = employeeModel.SignedProbationaryContract,
                Resignation = employeeModel.Resignation,
                PartnerId = employeeModel.PartnerId,
                ContactIds = employeeModel.ContactEmployees.Select(ce => ce.EmployeeId).ToList(),
            };
        }
    }
}