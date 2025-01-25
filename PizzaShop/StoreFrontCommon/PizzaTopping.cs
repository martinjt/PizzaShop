using System.ComponentModel.DataAnnotations.Schema;

namespace StoreFrontCommon;

/// <summary>
/// A topping added to a pizza (many-to-many)
/// </summary>
public class PizzaTopping
{
    public int PizzaId { get; set; }
    public int ToppingId { get; set; }
    public Topping? Topping { get; set; }
}