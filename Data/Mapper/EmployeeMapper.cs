
using Data.DTOs;
using Data.DTOs.Contact;
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
                Fullname = employeeModel.Fullname,
                PhoneNumber = employeeModel.PhoneNumber,
                StreetAddress = employeeModel.StreetAddress,
                District = employeeModel.District,
                Province = employeeModel.Province,
                Gender = employeeModel.Gender,
                DateOfBirth = employeeModel.DateOfBirth,
                Email = employeeModel.Email,
                JobTitle = employeeModel.JobTitle,
                PartnerId = employeeModel.PartnerId,
                ContactIds = employeeModel.Contacts.Select(c => c.Id).ToList(),
            };
        }
        public static EmployeeDTO ToAllEmployeeDTO(this Employee employeeModel)
        {
            return new EmployeeDTO
            {
                Fullname = employeeModel.Fullname,
                PhoneNumber = employeeModel.PhoneNumber,
                StreetAddress = employeeModel.StreetAddress,
                District = employeeModel.District,
                Province = employeeModel.Province,
                Gender = employeeModel.Gender,
                DateOfBirth = employeeModel.DateOfBirth,
                Email = employeeModel.Email,
                JobTitle = employeeModel.JobTitle,
                PartnerId = employeeModel.PartnerId,
                ContactIds = employeeModel.ContactEmployees.Select(ce => ce.EmployeeId).ToList(),
            };
        }
    }
}