using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaGateway;

public class KafkaMessagePumpService<TKey, TValue>(
    IConsumer<TKey, TValue> consumer,
    IEnumerable<string> topics,
    ILogger<KafkaMessagePumpService<int, string>> logger, 
    Func<TKey, TValue,  bool> handler) : IHostedService
{
    private readonly Channel<bool> _stop = Channel.CreateBounded<bool>(1);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var messagePump = new KafkaMessagePump<TKey, TValue>(consumer, topics, logger, _stop);
        
        //Kafka consumer is blocking, so we run it on a background thread. We use the channel to signal stopping
        //because the consumer does not understand cancellation tokens
        await Task.Run(() => messagePump.Run(handler), cancellationToken); 
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _stop.Writer.WriteAsync(true, cancellationToken);
    }
}