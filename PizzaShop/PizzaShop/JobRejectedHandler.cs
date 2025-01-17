using System.Threading.Channels;
using PizzaShop;

public class JobRejectedHandler(Channel<CourierStatusUpdate> courierStatusUpdates)
{
    public async Task<bool> HandleAsync(JobRejected jobRejected, CancellationToken token)
    {
        await courierStatusUpdates.Writer.WriteAsync(new CourierStatusUpdate(jobRejected.CourierId, CourierStatus.Rejected), token);
        return true;
    }
}