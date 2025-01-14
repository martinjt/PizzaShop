using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaGateway;

public class KafkaMessagePumpService<T>(
    IConsumer<string, T> consumer,
    string topic,
    ILogger<KafkaMessagePump<T>> logger,
    Func<T,bool> handler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var messagePump = new KafkaMessagePump<T>(consumer, topic);
        await messagePump.RunAsync(handler, stoppingToken);
    }
}