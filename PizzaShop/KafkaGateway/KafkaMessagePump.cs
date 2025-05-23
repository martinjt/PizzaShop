using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace KafkaGateway;

public class KafkaMessagePump<TKey, TValue, TRequest>(
    IConsumer<TKey, TValue> consumer, 
    IEnumerable<string> topics, 
    ILogger<KafkaMessagePumpService<TKey, TValue, TRequest>> logger,
    Channel<bool> stop)
{
    public void Run(Func<TValue, TRequest> mapper, Func<TRequest, bool> handler)
    {
        try
        {
            consumer.Subscribe(topics);
            
            while (true)
            {
                //end was signalled
                if (stop.Reader.TryRead(out _))
                    break;
                
                var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(100));

                if (consumeResult is null || consumeResult.IsPartitionEOF)
                {
                    logger.LogInformation("Kafka Message Pump: No message received. Waiting for 1 second.");
                    Task.Delay(1000).Wait();
                    continue;
                }
                
                logger.LogInformation($"Kafka Message Pump: Consumed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
                
                using var activity = consumeResult.StartProcessMessageActivity(typeof(TRequest));
                var request = mapper(consumeResult.Message.Value);
                var success = handler(request);
                if (success)
                {
                    //We don't want to commit unless we have successfully handled the message
                    //Commit directly. Normally we would want to batch these up, but for the demo we will
                    //commit after each message
                    consumer.Commit(consumeResult);
                    logger.LogInformation($"Kafka Message Pump: Committed message '{consumeResult.Message.Value}' at: '{consumeResult.TopicPartitionOffset}'.");
                }
            }
        }
        catch(KafkaException e)
        {
            logger.LogError(e, "Kafka Message Pump: Error consuming message");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Kafka message pump was cancelled");
            //Pump was cancelled, exit
        }
    }
}

public static class ConsumeResultExtensions
{
    public static Activity? StartProcessMessageActivity<TKey, TValue>(this ConsumeResult<TKey, TValue> consumeResult, Type messageType)
    {

        var propagationContext = Propagators.DefaultTextMapPropagator.Extract(
            new PropagationContext(Activity.Current?.Context ?? new ActivityContext(), Baggage.Current),
            consumeResult.Message.Headers, 
                (headers, name) => 
                    headers
                        .Where(h => h.Key == name)
                        .Select(h => Encoding.UTF8.GetString(h.GetValueBytes()))
        );
        return DiagnosticSettings.Source.StartActivity($"Process Event {messageType.Name}", ActivityKind.Consumer, propagationContext.ActivityContext);
    }
}