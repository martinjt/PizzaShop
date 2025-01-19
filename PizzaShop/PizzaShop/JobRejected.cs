namespace PizzaShop;

/// <summary>
/// A notification that a courier has rejected a job
/// </summary>
/// <param name="OrderId">The order we are being asked about</param>
/// <param name="CourierId">The courier rejecting the job</param>
public record JobRejected(int OrderId, string CourierId);