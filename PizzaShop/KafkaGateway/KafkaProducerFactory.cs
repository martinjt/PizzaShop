using System.Net;
using Confluent.Kafka;

namespace KafkaGateway;

public class KafkaProducerFactory<TKey, TValue>
{
    public static IProducer<TKey, TValue> Create(string bootStrapServer)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootStrapServer,
            EnableDeliveryReports = true,
            ClientId = Dns.GetHostName(),
            Acks = Acks.All,
            EnableIdempotence = false, 
            MessageSendMaxRetries = 3,
            MaxInFlight = 1,
        };
        
        //NOTE: Change to InstrumentedProducerBuilder to enable metrics
        var producerBuilder = new ProducerBuilder<TKey, TValue>(producerConfig);
        return producerBuilder.Build();
    }
}