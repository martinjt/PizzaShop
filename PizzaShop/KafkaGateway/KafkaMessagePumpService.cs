using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaGateway;

public class KafkaMessagePumpService<TKey, TValue>(
    IConsumer<TKey, TValue> consumer,
    string[] topics,
    ILogger<KafkaMessagePumpService<TKey, TValue>> logger,
    Func<TKey, TValue, Task<bool>> handler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting Kafka message pump service..."); 
        
        var messagePump = new KafkaMessagePump<TKey, TValue>(consumer, logger, topics);
        await messagePump.RunAsync(handler, stoppingToken);
        
        logger.LogInformation("Exited Kafka message pump service...");  
    }
}