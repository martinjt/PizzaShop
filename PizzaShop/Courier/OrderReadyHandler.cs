using System.Threading.Channels;

namespace Courier;

/// <summary>
/// Handles an order ready event by signalling that the order is ready for collection
/// Kicks off the courier's process of collecting the order
/// </summary>
/// <param name="courierName">The name of the courier, should be us</param>
/// <param name="deliveryJobs">The queue of our waiting deliveryJobs</param>
internal class OrderReadyHandler(string courierName, Channel<OrderStatus> deliveryJobs)
{
    public async Task<bool> HandleAsync(OrderReady job, CancellationToken token)
    {
        // is the job actually for us? As we listen on courier specific queues, this is bad configuration if not
        if (job.CourierId != courierName)
            return false;
        
        var orderStatus = CreateOrderStatus(job);
        await  deliveryJobs.Writer.WriteAsync(orderStatus, token);
        return true;
    }

    private OrderStatus CreateOrderStatus(OrderReady job)
    {
        var orderStatus = new OrderStatus
        {
            CourierId = courierName,
            OrderId = job.OrderId,
            ETA = GetETA(DateTimeOffset.UtcNow),
            Status = "Ready"
        };
        return orderStatus;
    }

    private DateTimeOffset GetETA(DateTimeOffset originalTime)
    {
        return originalTime.AddMinutes(15);
    }
}