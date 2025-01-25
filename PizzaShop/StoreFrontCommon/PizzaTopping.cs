namespace StoreFrontCommon;

/// <summary>
/// A topping added to a pizza (many-to-many)
/// </summary>
public class PizzaTopping
{
    public int PizzaToppingId { get; set; }
    public int PizzaId { get; set; }
    public int ToppingId { get; set; }
    public Topping? Topping { get; set; }
}