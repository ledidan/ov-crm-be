using System.Collections.Generic;
using Data.DTOs;
using Data.MongoModels;

public class CreateOrderDTO
{
    public required OrderDTO Order { get; set; }
    // public List<OrderDetailDTO> OrderDetails { get; set; } = new();
}