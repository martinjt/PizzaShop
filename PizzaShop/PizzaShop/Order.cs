using Microsoft.Azure.Amqp.Framing;

namespace PizzaShop;

public class Order
{
    public int OrderId { get; set; }

    public DateTime CreatedTime { get; set; }

    public Address DeliveryAddress { get; set; } = new Address();

    public List<Pizza> Pizzas { get; set; } = new List<Pizza>();
}
