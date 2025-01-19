namespace PizzaShop;

/// <summary>
/// A notification that an order is ready for pickup
/// </summary>
/// <param name="OrderId">The order that is ready</param>
/// <param name="CourierId">The assigned courier</param>
public record OrderReady(int OrderId, string CourierId);