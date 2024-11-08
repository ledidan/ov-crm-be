using Data.DTOs;
using Data.Entities;
using Data.Responses;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Interfaces;

namespace ServerLibrary.Services.Implementations
{
    public class CustomerService(AppDbContext appDbContext,
        IPartnerService partnerService) : ICustomerService
    {
        public async Task<GeneralResponse> CreateAsync(CreateCustomer customer)
        {
            if (customer == null) return new GeneralResponse(false, "Model is empty");

            //check partner
            var partner = await partnerService.FindById(customer.PartnerId);
            if (partner == null) return new GeneralResponse(false, "Partner not found");

            await appDbContext.InsertIntoDb(new Customer()
            {
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                StreetAddress = customer.StreetAddress,
                District = customer.District,
                Province = customer.Province,
                Partner = partner,
            });

            return new GeneralResponse(true, "Customer created");
        }

        public async Task<List<Customer>> GetAllAsync(Partner partner)
        {
            var result = await appDbContext.Customers.Where(_ => _.Partner.Id == partner.Id).ToListAsync();
            return result;
        }

        public async Task<GeneralResponse> UpdateAsync(Customer customer)
        {
            //check customer
            var customerUpdating = await appDbContext.Customers.FirstOrDefaultAsync(_ => _.Id == customer.Id);
            if (customerUpdating == null) return new GeneralResponse(false, "Customer not found");

            await appDbContext.UpdateDb(customer);
            return new GeneralResponse(true, "Customer updated successfully");
        }
    }
}
