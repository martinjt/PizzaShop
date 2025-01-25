using Confluent.Kafka;
using KafkaGateway;
using Microsoft.EntityFrameworkCore;
using StoreFrontCommon;

namespace StoreFrontWorker;

internal static class AfterOrderServiceFactory
{
    public static KafkaMessagePumpService<int, string> Create(string[] topics, IServiceProvider serviceProvider)
    {
        var consumer = serviceProvider.GetRequiredService<IConsumer<int, string>>();
        if (consumer is null) throw new InvalidOperationException("No Kafka Consumer registered");
    
        var logger = serviceProvider.GetRequiredService<ILogger<KafkaMessagePumpService<int, string>>>();
        
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<PizzaShopDb>>();
        
        return new KafkaMessagePumpService<int, string>(
            consumer, 
            topics, 
            logger,
            (key, value) => new AfterOrderService(dbContextFactory).Handle(new OrderStatusChange(key, value)));
    }
}