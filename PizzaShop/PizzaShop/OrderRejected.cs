namespace PizzaShop;

/// <summary>
/// Signals that an order has been rejected
/// </summary>
/// <param name="OrderId">The order we are rejecting</param>
/// <param name="CourierId">The courier doing the rejecting</param>
public record OrderRejected(int OrderId, string CourierId);