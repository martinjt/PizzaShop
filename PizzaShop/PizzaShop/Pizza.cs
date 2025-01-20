using System.Text.Json.Serialization;

namespace PizzaShop;

/// <summary>
/// A pizza in an order. A pizza has a size and toppings
/// </summary>
public class Pizza
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public PizzaSize Size { get; set; }
    public List<PizzaTopping> Toppings { get; set; } = new();
}