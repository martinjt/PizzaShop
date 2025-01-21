using System.ComponentModel;

namespace StoreFront;

/// <summary>
/// A topping that can be added to a pizza
/// </summary>
public class Topping
{
    [Description("The Id for this topping")]
    public int ToppingId { get; set; }
    [Description("The name of the topping")]
    public string Name { get; set; } = string.Empty;
    [Description("How much does this topping cost?")]
    public decimal Price { get; set; }

}