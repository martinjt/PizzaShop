using System.Threading.Channels;

namespace PizzaShop;

public class JobAcceptedHandler(Channel<CourierStatusUpdate> courierStatusUpdates)
{
    public async Task<bool> HandleAsync(JobAccepted jobAccepted, CancellationToken token)
    {
        await courierStatusUpdates.Writer.WriteAsync(new CourierStatusUpdate(jobAccepted.CourierId, CourierStatus.Accepted), token);
        return true;
    }
}