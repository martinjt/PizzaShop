using Azure.Messaging.ServiceBus;

namespace AsbGateway;

/// <summary>
/// A producer sends messages. This class will deserialize an internal message type, which contains
/// an instance of a payload of generic type, into a service bus message
/// </summary>
/// <param name="busClient">The service bus client</param>
/// <param name="serviceBusMessageMapper">A mapper function to turn a message of type T into a service bus message</param>
public class AsbProducer<T>(ServiceBusClient? busClient, string queueName, Func<Message<T>, ServiceBusMessage> mapToServiceBusMessage) 
    : AsbGateway<T>(busClient)
{
    public async Task SendMessageAsync(Message<T> message, CancellationToken cancellationToken = default)
    {
        if (BusClient is null)
            throw new InvalidOperationException("Bus client is not initialized");
        
        await using var sender = BusClient.CreateSender(queueName);
        await sender.SendMessageAsync(mapToServiceBusMessage(message), cancellationToken);
    }
}