using System.ComponentModel;

namespace StoreFront;

/// <summary>
/// An order for our pizza shop
/// </summary>
public class Order
{
    [Description("The unique identifier for this order")]
    public int Id { get; set; }
    [Description("What time was the order placed?")]
    public DateTimeOffset CreatedTime { get; set; }
    [Description("Where do you want this delivered?")]
    public Address DeliveryAddress { get; set; } = new Address();
    [Description("What pizzas do you want?")]
    public List<Pizza> Pizzas { get; set; } = new List<Pizza>();
    [Description("What is the status of this order?")]
    public OrderStatus Status { get; set; }
}
