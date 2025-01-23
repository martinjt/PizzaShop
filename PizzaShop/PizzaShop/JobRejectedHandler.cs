using System.Threading.Channels;

namespace PizzaShop;

/// <summary>
/// When a courier rejects a job, we signal to the kitchen that the courier has rejected the job
/// </summary>
/// <param name="courierStatusUpdates">The channel to send courier status updates to - we produce to this</param>
internal class JobRejectedHandler(Channel<CourierStatusUpdate> courierStatusUpdates)
{
    public async Task<bool> HandleAsync(JobRejected jobRejected, CancellationToken token)
    {
        await courierStatusUpdates.Writer.WriteAsync(new CourierStatusUpdate(jobRejected.CourierId, CourierStatus.Rejected), token);
        return true;
    }
}