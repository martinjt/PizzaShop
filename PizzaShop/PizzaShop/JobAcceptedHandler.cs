using System.Threading.Channels;

namespace PizzaShop;

/// <summary>
/// When a courier accepts a job, we signal to the kitchen that they can notify them to pick up the delivery
/// </summary>
/// <param name="courierStatusUpdates">The channel for courier status updates - we produce to this</param>
public class JobAcceptedHandler(Channel<CourierStatusUpdate> courierStatusUpdates)
{
    public async Task<bool> HandleAsync(JobAccepted jobAccepted, CancellationToken token)
    {
        await courierStatusUpdates.Writer.WriteAsync(new CourierStatusUpdate(jobAccepted.CourierId, CourierStatus.Accepted), token);
        return true;
    }
}