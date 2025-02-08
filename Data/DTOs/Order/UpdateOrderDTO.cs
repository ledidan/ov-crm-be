using Data.DTOs.Order;

public class UpdateOrderDTO
{
    public OrderDTO Order { get; set; }
    public List<OrderDetailDTO> OrderDetails { get; set; }
}
