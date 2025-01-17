namespace PizzaShop;

public class CookRequest
{
    public CookRequest(Order order)
    {
        OrderId = order.OrderId;
        Pizzas = order.Pizzas;
    }

    public int CookId { get; set; }
    public int OrderId { get; set; }
    public List<Pizza> Pizzas { get; set; }
}