namespace PizzaShop;

/// <summary>
/// A job that a courier has accepted
/// </summary>
/// <param name="OrderId">The id of the order </param>
/// <param name="CourierId">The ide of the courier</param>
public record JobAccepted(int OrderId, string CourierId);