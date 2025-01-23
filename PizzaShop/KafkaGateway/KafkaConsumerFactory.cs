using Confluent.Kafka;

namespace KafkaGateway;

public static class KafkaConsumerFactory<TKey,TValue>
{

    public static ConsumerBuilder<TKey, TValue> Create(string bootstrapServer, string groupId)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = bootstrapServer,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            //track offsets but we will ack them manually
            EnableAutoOffsetStore = true,
            EnableAutoCommit = false,
        };

        //NOTE: Change to InstrumentedConsumerBuilder to enable metrics
        var consumerBuilder = new ConsumerBuilder<TKey, TValue>(consumerConfig);
        return consumerBuilder.AsInstrumentedConsumerBuilder();
    }
}