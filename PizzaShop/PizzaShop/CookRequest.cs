using Shared;

namespace PizzaShop;

/// <summary>
/// A request to cook an order
/// </summary>
/// <param name="order">The order we are cooking</param>
public class CookRequest(Order order) : TraceableRequest
{
    public int OrderId { get; set; } = order.OrderId;
    public List<Pizza> Pizzas { get; set; } = order.Pizzas;
}