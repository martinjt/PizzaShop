using Microsoft.Azure.Amqp.Framing;

namespace PizzaShop;

/// <summary>
/// An order for our pizza shop
/// </summary>
public class Order
{
    public int OrderId { get; set; }
    public DateTime CreatedTime { get; set; }
    public Address DeliveryAddress { get; set; } = new Address();
    public Address PickupAddress { get; set; } = new Address();
    public List<Pizza> Pizzas { get; set; } = new List<Pizza>();
}
