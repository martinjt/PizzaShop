using System.Diagnostics;
using Confluent.Kafka;

namespace KafkaGateway;

public class KafkaMessagePump<TKey, TValue>(IConsumer<TKey, TValue> consumer, string[] topics)
{
    public async Task RunAsync(
        Func<TKey, TValue, Task<bool>> handler, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            consumer.Subscribe(topics);
            
            while (!cancellationToken.IsCancellationRequested) 
            {
                var consumeResult = consumer.Consume(cancellationToken);

                if (consumeResult.IsPartitionEOF)
                {
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }
                
                Activity.Current?.AddEvent(new ActivityEvent("KafkaMessageReceived", tags: new ActivityTagsCollection
                {
                    ["Topic"] = consumeResult.Topic,
                    ["Partition"] = consumeResult.Partition.Value,
                    ["Offset"] = consumeResult.Offset.Value
                }));
                
                var success = await handler(consumeResult.Message.Key, consumeResult.Message.Value);
                if (success)
                {
                    //We don't want to commit unless we have successfully handled the message
                    //Commit directly. Normally we would want to batch these up, but for the demo we will
                    //commit after each message
                    consumer.Commit(consumeResult);
                }
            }
        }
        catch(KafkaException kfe)
        {
            Activity.Current?.AddEvent(new ActivityEvent("KafkaException", tags: new ActivityTagsCollection
            {
                ["Error"] = kfe.Error.Reason,
                ["IsFatal"] = kfe.Error.IsFatal
            }));    
        }
        catch (OperationCanceledException)
        {
            //Pump was cancelled, exit
        }
    }
} 