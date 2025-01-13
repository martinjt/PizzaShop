using Azure.Messaging.ServiceBus;

namespace AsbGateway;

public class AsbConsumer<T>(
    string queueName,
    ServiceBusClient? busClient,
    Func<ServiceBusReceivedMessage, T> mapToRequest,
    TimeSpan? maxWaitTime = null)
    : AsbGateway<T>(busClient)
{
    private ServiceBusReceiver? _receiver;

    public void Init()
    {
        if (BusClient is null)
            throw new InvalidOperationException("Bus client is not initialized");
        
        _receiver = BusClient.CreateReceiver(
             queueName, 
             new ServiceBusReceiverOptions{ ReceiveMode = ServiceBusReceiveMode.PeekLock }
             );
    }

    public async Task<Request<T>?> ReceiveMessage(CancellationToken cancellationToken = default)
    {
        if (_receiver is null)
            throw new InvalidOperationException("Receiver is not initialized; did you forget to call Init()?");
        
        var message = await _receiver.ReceiveMessageAsync(maxWaitTime, cancellationToken);
        return message is null ? null : new Request<T>(mapToRequest(message), message);
    }

    public async Task CompleteMessage(Request<T> message)
    {
        if (_receiver is null)
            throw new InvalidOperationException("Receiver is not initialized; did you forget to call Init()?");
        
        await _receiver.CompleteMessageAsync(message.ReceivedMessage);
    }
}