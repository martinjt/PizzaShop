using Azure.Messaging.ServiceBus;

namespace AsbGateway;

/// <summary>
/// Creates a message pump that will read, translate and dispatch a message from an ASB queue
/// </summary>
/// <param name="busClient">The client for Azure Service Bus</param>
/// <param name="queueName">The queue we are reading</param>
/// <param name="mapToRequest">A function to map a ServiceBusMessage to a <see cref="Request{T}"/> type</param>
/// <param name="handler">A function to handle the content of the <see cref="Request{T}"/> type</param>
/// <typeparam name="T"></typeparam>
public class AsbMessagePump<T>(
    ServiceBusClient busClient,
    string queueName,
    Func<ServiceBusReceivedMessage, T> mapToRequest,
    Func<T, CancellationToken, Task<bool>> handler)
{
    private readonly AsbConsumer<T> _consumer = new(queueName, busClient, mapToRequest);

    public async Task Run(CancellationToken cancellationToken = default)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            var request = await _consumer.ReceiveMessage(cancellationToken);
            if (request is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                continue;
            }
            
            if (await handler(request.Content, cancellationToken))
                await _consumer.CompleteMessage(request);
        }
    }
}