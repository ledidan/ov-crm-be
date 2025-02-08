using System.Collections.Generic;
using Data.DTOs.Order;
using Data.MongoModels;

public class CreateOrderDTO
{
    public OrderDTO Order { get; set; }
    public List<OrderDetailDTO> OrderDetails { get; set; }
}
