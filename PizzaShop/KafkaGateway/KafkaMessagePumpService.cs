using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaGateway;

public class KafkaMessagePumpService<TKey, TValue>(
    IConsumer<TKey, TValue> consumer,
    string[] topics,
    ILogger<KafkaMessagePump<TKey, TValue>> logger,
    Func<TKey, TValue, Task<bool>> handler) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try {
            var messagePump = new KafkaMessagePump<TKey, TValue>(consumer, topics, logger);
            return Task.Run(() => messagePump.RunAsync(handler, stoppingToken), stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting the MessagePump");
            throw;
        }
    }
}