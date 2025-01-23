using Shared;

namespace PizzaShop;

//A request to deliver an order
public class DeliveryRequest(string courierId, Order order) : TraceableRequest {
    public string CourierId { get; set; } = courierId;
    public int OrderId { get; set; } = order.OrderId;
    public Address DeliveryAddress { get; set; } = order.DeliveryAddress;
    public Address PickupAddress { get; set; } = order.PickupAddress;
}