using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AsbGateway;

public class AsbMessagePumpService<T>(
    ServiceBusClient client,
    string queueName,
    ILogger<AsbMessagePump<T>> logger,
    Func<ServiceBusReceivedMessage, T> mapToRequest,
    Func<T,CancellationToken,Task<bool>> handler)
    : BackgroundService 
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messagePump = new AsbMessagePump<T>(client, queueName, mapToRequest, handler, logger);
        return messagePump.Run(stoppingToken); 
    }
}