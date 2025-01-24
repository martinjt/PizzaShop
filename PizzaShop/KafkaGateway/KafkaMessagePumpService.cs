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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messagePump = new KafkaMessagePump<TKey, TValue>(consumer, topics);
        await messagePump.RunAsync(handler, stoppingToken);
    }
}