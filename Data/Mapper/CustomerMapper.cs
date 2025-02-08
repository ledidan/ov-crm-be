using Data.DTOs;
using Data.Entities;

namespace Mapper.CustomerMapper
{
    public static class CustomerMapper
    {
        public static CustomerDTO ToCustomerDTO(this Customer customerModel)
        {
            return new CustomerDTO
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
                ContactIDAim = customerModel.ContactIDAim,
                CelebrateDate = customerModel.CelebrateDate,
                OwnerID = customerModel.OwnerID,
                OfficeEmail = customerModel.OfficeEmail,
                OfficeTel = customerModel.OfficeTel,
                OrganizationUnitID = customerModel.OrganizationUnitID,
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
                RevenueDetail = customerModel.RevenueDetail,
                BusinessTypeID = customerModel.BusinessTypeID,
                ContactIDAim = customerModel.ContactIDAim,
                CelebrateDate = customerModel.CelebrateDate,
                CustomerSinceDate = customerModel.CustomerSinceDate,
                OwnerID = customerModel.OwnerID,
                OfficeEmail = customerModel.OfficeEmail,
                OfficeTel = customerModel.OfficeTel,
                OrganizationUnitID = customerModel.OrganizationUnitID,
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
                IsDistributor = customerModel.IsDistributor
            };
        }
    }
}