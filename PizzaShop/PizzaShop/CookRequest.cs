namespace PizzaShop;

public class CookRequest
{
    public CookRequest(Order order)
    {
        OrderId = order.OrderId;
    }

    public int CookId { get; set; }
    public int OrderId { get; set; }
}