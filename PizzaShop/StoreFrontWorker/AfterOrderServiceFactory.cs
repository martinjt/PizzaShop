using System.Text.Json;
using Confluent.Kafka;
using KafkaGateway;
using Microsoft.EntityFrameworkCore;
using StoreFrontCommon;

namespace StoreFrontWorker;

internal static class AfterOrderServiceFactory
{
    public static KafkaMessagePumpService<int, string, OrderStatusChange> Create(string[] topics, IServiceProvider serviceProvider)
    {
        var consumer = serviceProvider.GetRequiredService<IConsumer<int, string>>();
        if (consumer is null) throw new InvalidOperationException("No Kafka Consumer registered");
    
        var logger = serviceProvider.GetRequiredService<ILogger<KafkaMessagePumpService<int, string, OrderStatusChange>>>();
        
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<PizzaShopDb>>();
        
        return new KafkaMessagePumpService<int, string, OrderStatusChange>(
            consumer, 
            topics, 
            logger,
            (value) => JsonSerializer.Deserialize<OrderStatusChange>(value) ?? new OrderStatusChange(), 
            (orderstatuschange) => new AfterOrderService(dbContextFactory).Handle(orderstatuschange));
    }
}