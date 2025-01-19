namespace PizzaShop;

/// <summary>
/// A topping that can be added to a pizza
/// </summary>
public class Topping
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

}