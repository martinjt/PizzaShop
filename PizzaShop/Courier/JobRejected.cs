namespace Courier;

/// <summary>
/// The job has been rejected
/// </summary>
/// <param name="OrderId">The order that the job is for</param>
/// <param name="CourierId">The id of the courier assigned to the job - us</param>
public record JobRejected(int OrderId, string CourierId);