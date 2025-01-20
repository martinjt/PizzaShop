namespace StoreFront;

/// <summary>
/// Listens to status updates about an order
/// Normally, we would tend to run a Kafka worker in a separate process, so that we could scale out to the number of
/// partitions we had, separate to scaling for the number of HTTP requests.
/// To make this simpler, for now, we are just running it as a background process, as we don't need to scale it
/// </summary>
public class AfterOrderService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}