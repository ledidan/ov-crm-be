using Data.DTOs.Order;
using Data.MongoModels;
using MongoDB.Driver;

namespace Mapper.OrderMapper
{
    public static class OrderMapper
    {
        public static Orders ToCustomerFromCreateDTO(this OrderDTO orderModel)
        {
            return new Orders
            {   
                TotalAmount = orderModel.TotalAmount,
                IsPaid = orderModel.IsPaid,
                IsShared = orderModel.IsShared,
                PaidDate = orderModel.PaidDate,
                PartnerId = orderModel.PartnerId,
            };
        }
    }
}