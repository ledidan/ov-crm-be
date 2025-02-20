using Data.DTOs;
using Data.Entities;
using Data.MongoModels;
using MongoDB.Driver;

namespace Mapper.PartnerMapper
{
    public static class PartnerMapper
    {
        public static Partner ToCustomerFromCreateDTO(this PartnerDTO orderModel)
        {
            return new Partner
            {   
                // TotalAmount = orderModel.TotalAmount,
                // IsPaid = orderModel.IsPaid,
                // IsShared = orderModel.IsShared,
                // PaidDate = orderModel.PaidDate,
                // PartnerId = orderModel.PartnerId,
            };
        }
    }
}