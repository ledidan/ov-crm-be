using Data.DTOs;
using Data.Entities;

namespace Mapper.CustomerMapper
{
    public static class CustomerMapper
    {
        public static OptionalCustomerDTO ToCustomerDTO(this Customer customerModel)
        {
            return new OptionalCustomerDTO
            {
                Id = customerModel.Id,
                AccountName = customerModel.AccountName,
                AccountNumber = customerModel.AccountNumber,
                AccountReferredID = customerModel.AccountReferredID,
                AccountShortName = customerModel.AccountShortName,
                AccountTypeID = customerModel.AccountTypeID,
                BankAccount = customerModel.BankAccount,
                BankName = customerModel.BankName,
                BillingCode = customerModel.BillingCode,
                BillingCountryID = customerModel.BillingCountryID,
                BillingDistrictID = customerModel.BillingDistrictID,
                BillingProvinceID = customerModel.BillingProvinceID,
                CustomerSinceDate = customerModel.CustomerSinceDate,
                BillingStreet = customerModel.BillingStreet,
                BillingWardID = customerModel.BillingWardID,
                BudgetCode = customerModel.BudgetCode,
                RevenueDetail = customerModel.RevenueDetail,
                BusinessTypeID = customerModel.BusinessTypeID,
                CelebrateDate = customerModel.CelebrateDate,
                OwnerID = customerModel.OwnerID,
                OfficeEmail = customerModel.OfficeEmail,
                OfficeTel = customerModel.OfficeTel,
                SectorText = customerModel.SectorText,
                ShippingCode = customerModel.ShippingCode,
                ShippingCountryID = customerModel.ShippingCountryID,
                ShippingDistrictID = customerModel.ShippingDistrictID,
                ShippingProvinceID = customerModel.ShippingProvinceID,
                ShippingStreet = customerModel.ShippingStreet,
                ShippingWardID = customerModel.ShippingWardID,
                NumberOfDaysOwed = customerModel.NumberOfDaysOwed,
                AnnualRevenueID = customerModel.AnnualRevenueID,
                TaxCode = customerModel.TaxCode,
                Website = customerModel.Website,
                Avatar = customerModel.Avatar,
                Description = customerModel.Description,
                IndustryID = customerModel.IndustryID,
                IsPublic = customerModel.IsPublic,
                IsPartner = customerModel.IsPartner,
                IsPersonal = customerModel.IsPersonal,
                IsOldCustomer = customerModel.IsOldCustomer,
                IsDistributor = customerModel.IsDistributor,
                OwnerIDName = customerModel.OwnerIDName,
                EmployeeCode = customerModel.Employee?.EmployeeCode,
                EmployeeName = customerModel.Employee?.FullName,
                CustomerEmployees = customerModel.CustomerEmployees?.Select(e => new CustomerEmployees
                {
                    CustomerId = e.CustomerId,
                    EmployeeId = e.EmployeeId,
                    PartnerId = e.PartnerId
                }).ToList() ?? new List<CustomerEmployees>(),
                CustomerContacts = customerModel.CustomerContacts?.Select(e => new CustomerContacts
                {
                    CustomerId = e.CustomerId,
                    ContactId = e.ContactId,
                    PartnerId = e.PartnerId
                }).ToList() ?? new List<CustomerContacts>()
            };
        }
        public static Customer ToCustomerFromUpdateDTO(this UpdateCustomerDTO customerModel)
        {
            return new Customer
            {
                AccountName = customerModel.AccountName,
                AccountNumber = customerModel.AccountNumber,
                AccountReferredID = customerModel.AccountReferredID,
                AccountShortName = customerModel.AccountShortName,
                AccountTypeID = customerModel.AccountTypeID,
                BankAccount = customerModel.BankAccount,
                BankName = customerModel.BankName,
                BillingCode = customerModel.BillingCode,
                BillingCountryID = customerModel.BillingCountryID,
                BillingDistrictID = customerModel.BillingDistrictID,
                BillingProvinceID = customerModel.BillingProvinceID,
                BillingStreet = customerModel.BillingStreet,
                BillingWardID = customerModel.BillingWardID,
                BudgetCode = customerModel.BudgetCode,
                Debt = customerModel.Debt,
                DebtLimit = customerModel.DebtLimit,
                RevenueDetail = customerModel.RevenueDetail,
                BusinessTypeID = customerModel.BusinessTypeID,
                CelebrateDate = customerModel.CelebrateDate,
                CustomerSinceDate = customerModel.CustomerSinceDate,
                OwnerID = customerModel.OwnerID,
                OfficeEmail = customerModel.OfficeEmail,
                OfficeTel = customerModel.OfficeTel,
                AnnualRevenueID = customerModel.AnnualRevenueID,
                SectorText = customerModel.SectorText,
                ShippingCode = customerModel.ShippingCode,
                ShippingCountryID = customerModel.ShippingCountryID,
                ShippingDistrictID = customerModel.ShippingDistrictID,
                ShippingProvinceID = customerModel.ShippingProvinceID,
                ShippingStreet = customerModel.ShippingStreet,
                ShippingWardID = customerModel.ShippingWardID,
                TaxCode = customerModel.TaxCode,
                Website = customerModel.Website,
                Avatar = customerModel.Avatar,
                Description = customerModel.Description,
                NumberOfDaysOwed = customerModel.NumberOfDaysOwed,
                IndustryID = customerModel.IndustryID,
                IsPublic = customerModel.IsPublic,
                IsPartner = customerModel.IsPartner,
                IsPersonal = customerModel.IsPersonal,
                IsOldCustomer = customerModel.IsOldCustomer,
                IsDistributor = customerModel.IsDistributor,
            };
        }
    }
}