using System.Text.Json.Serialization;

namespace PizzaShop;

public class Pizza
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Size { get; set; }
    public List<PizzaTopping> Toppings { get; set; } = new();
}