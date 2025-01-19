namespace PizzaShop;

/// <summary>
/// A topping added to a pizza (many-to-many)
/// </summary>
public class PizzaTopping
{
    public Topping? Topping { get; set; }
    public int ToppingId { get; set; }
    public int PizzaId { get; set; }

}