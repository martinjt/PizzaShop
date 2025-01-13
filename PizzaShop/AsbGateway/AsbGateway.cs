using Azure.Messaging.ServiceBus;

namespace AsbGateway;

/// <summary>
/// abstracts a connection to Asb
/// </summary>
public class AsbGateway<T>(ServiceBusClient? busClient) : IDisposable, IAsyncDisposable
{
    protected ServiceBusClient? BusClient { get; set; } = busClient;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    { 
        //do nothing as bus client is async disposable only 
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (BusClient is not null)
        {
            await BusClient.DisposeAsync().ConfigureAwait(false);
        }

        BusClient = null;
    }
}