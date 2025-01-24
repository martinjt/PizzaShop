using Confluent.Kafka;
using KafkaGateway;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace StoreFront;

internal static class AfterOrderServiceFactory
{
    public static KafkaMessagePumpService<int, string> Create(string[] topics, IServiceProvider serviceProvider)
    {
        var consumer = serviceProvider.GetRequiredService<IConsumer<int, string>>();
        if (consumer is null) throw new InvalidOperationException("No Kafka Consumer registered");
    
        var logger = serviceProvider.GetRequiredService<ILogger<KafkaMessagePump<int, string>>>();
        var db = serviceProvider.GetService<PizzaShopDb>();
        
        return new KafkaMessagePumpService<int, string>(
            consumer, 
            topics, 
            logger,
            (key, value) => new AfterOrderService(db).HandleAsync(key, value));
    }
}