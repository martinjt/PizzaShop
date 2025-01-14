namespace PizzaShop;

public class DeliveryRequest
{
    public DeliveryRequest(Order order)
    {
        OrderId = order.OrderId;
    }

    public int CourierId { get; set; }
    public int OrderId { get; set; }
}