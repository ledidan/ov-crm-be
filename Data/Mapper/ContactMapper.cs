using Data.DTOs;
using Data.Entities;
using Mapper.EmployeeMapper;

namespace Mapper.ContactMapper
{
    public static class ContactMapper
    {
        public static ContactDTO ToContactDTO(this Contact contactModel)
        {
            return new ContactDTO
            {
                Id = contactModel.Id,
                AccountTypeID = contactModel.AccountTypeID,
                ContactCode = contactModel.ContactCode,
                ContactName = contactModel.ContactName,
                DepartmentID = contactModel.DepartmentID,
                LeadSourceID = contactModel.LeadSourceID,
                FirstName = contactModel.FirstName,
                LastName = contactModel.LastName,
                Email = contactModel.Email,
                OfficeTel = contactModel.OfficeTel,
                DateOfBirth = contactModel.DateOfBirth,
                Description = contactModel.Description,
                MailingDistrictID = contactModel.MailingDistrictID,
                MailingProvinceID = contactModel.MailingProvinceID,
                MailingStreet = contactModel.MailingStreet,
                MailingWardID = contactModel.MailingWardID,
                MailingZip = contactModel.MailingZip,
                Mobile = contactModel.Mobile,
                OfficeEmail = contactModel.OfficeEmail,
                OtherPhone = contactModel.OtherPhone,
                SalutationID = contactModel.SalutationID,
                ShippingDistrictID = contactModel.ShippingDistrictID,
                ShippingProvinceID = contactModel.ShippingProvinceID,
                ShippingStreet = contactModel.ShippingStreet,
                ShippingWardID = contactModel.ShippingWardID,
                ShippingZip = contactModel.ShippingZip,
                TitleID = contactModel.TitleID,
                Zalo = contactModel.Zalo,
                IsPublic = contactModel.IsPublic,
                EmailOptOut = contactModel.EmailOptOut,
                PhoneOptOut = contactModel.PhoneOptOut,
            };
        }

        public static UpdateContactDTO FromUpdateContactDTO(this UpdateContactDTO contactModel)
        {
            return new UpdateContactDTO
            {   
                
                ContactName = contactModel.ContactName,
                FirstName = contactModel.FirstName,
                LastName = contactModel.LastName,
                Email = contactModel.Email,
                OfficeTel = contactModel.OfficeTel,
                DateOfBirth = contactModel.DateOfBirth,
                Description = contactModel.Description,
                ShippingAddress = contactModel.ShippingAddress,
                 AccountTypeID = contactModel.AccountTypeID,
                ContactCode = contactModel.ContactCode,
                DepartmentID = contactModel.DepartmentID,
                LeadSourceID = contactModel.LeadSourceID,
                MailingDistrictID = contactModel.MailingDistrictID,
                MailingProvinceID = contactModel.MailingProvinceID,
                MailingStreet = contactModel.MailingStreet,
                MailingWardID = contactModel.MailingWardID,
                MailingZip = contactModel.MailingZip,
                Mobile = contactModel.Mobile,
                OfficeEmail = contactModel.OfficeEmail,
                OtherPhone = contactModel.OtherPhone,
                SalutationID = contactModel.SalutationID,
                ShippingDistrictID = contactModel.ShippingDistrictID,
                ShippingProvinceID = contactModel.ShippingProvinceID,
                ShippingStreet = contactModel.ShippingStreet,
                ShippingWardID = contactModel.ShippingWardID,
                ShippingZip = contactModel.ShippingZip,
                TitleID = contactModel.TitleID,
                Zalo = contactModel.Zalo,
                IsPublic = contactModel.IsPublic,
                EmailOptOut = contactModel.EmailOptOut,
                PhoneOptOut = contactModel.PhoneOptOut,
            };
        }
    }
}