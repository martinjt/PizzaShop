namespace StoreFront;

/// <summary>
/// A topping added to a pizza (many-to-many)
/// </summary>
public class PizzaTopping
{
    public int Id { get; set; }
    public Topping? Topping { get; set; }
    public int ToppingId { get; set; }
    public int PizzaId { get; set; }

}