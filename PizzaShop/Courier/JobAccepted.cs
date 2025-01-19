namespace Courier;

/// <summary>
/// Indicates that a job has been accepted
/// </summary>
/// <param name="OrderId">The order we are talking about</param>
/// <param name="CourierId">The id of the courier that has accepted the job - us</param>
public record JobAccepted(int OrderId, string CourierId);