using System.Diagnostics;
using Confluent.Kafka;

namespace KafkaGateway;

public class KafkaMessagePump<TKey, TValue>(IConsumer<TKey, TValue> consumer, string topic)
{
    public async Task RunAsync(
        Func<TKey, TValue, Task<bool>> handler, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            consumer.Subscribe(topic);
            
            while (true) 
            {
                var consumeResult = consumer.Consume(cancellationToken);

                if (consumeResult.IsPartitionEOF)
                {
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }
                
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
        catch(ConsumeException e)
        {
            Debug.WriteLine(e);
        }
        catch (OperationCanceledException)
        {
            //Pump was cancelled, exit
        }
    }
} 