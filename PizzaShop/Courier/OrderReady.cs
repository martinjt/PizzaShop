namespace Courier;

/// <summary>
/// A message indicating that an order is ready for collection
/// </summary>
/// <param name="OrderId">The order that needs collection</param>
/// <param name="CourierId">The courier assigned to collect it - us</param>
public record OrderReady(int OrderId, string CourierId);