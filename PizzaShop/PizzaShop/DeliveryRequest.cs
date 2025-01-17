namespace PizzaShop;

public class DeliveryRequest
{
    public DeliveryRequest(Order order)
    {
        OrderId = order.OrderId;
        DeliveryAddress = order.DeliveryAddress;
        PickupAddress = order.PickupAddress;
    }

    public int CourierId { get; set; }
    public int OrderId { get; set; }
    public Address DeliveryAddress { get; set; }
    public Address PickupAddress { get; set; }
}