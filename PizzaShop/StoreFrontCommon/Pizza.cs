using System.ComponentModel;

namespace StoreFrontCommon;

/// <summary>
/// A pizza in an order. A pizza has a size and toppings
/// </summary>
public class Pizza
{
    public int PizzaId { get; set; }
    [Description("What order does this pizza belong to?")]
    public int OrderId { get; set; }
    [Description("How large should this pizza be, in inches?")]
    public PizzaSize Size { get; set; }
}