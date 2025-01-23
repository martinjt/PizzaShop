using Confluent.Kafka;
using KafkaGateway;
using Microsoft.EntityFrameworkCore;

namespace StoreFront;

internal static class OrderServiceFactory
{
    public static KafkaMessagePumpService<int, string> Create(string queueName, IServiceProvider serviceProvider)
    {
        var consumer = serviceProvider.GetService<IConsumer<int, string>>();
        if (consumer is null) throw new InvalidOperationException("No Kafka Consumer registered");
    
        var logger = serviceProvider.GetRequiredService<ILogger<KafkaMessagePump<int, string>>>();
    
        return new KafkaMessagePumpService<int, string>(
            consumer, 
            queueName, 
            logger,
            (async (key, value) =>
            {
                var orderId = key;
                var orderStatus = Enum.Parse<OrderStatus>(value);
            
                {
                    var db = serviceProvider.GetService<PizzaShopDb>();
                    if (db is null) throw new InvalidOperationException("No  EF Context");
                
                    var orderToUpdate = await db.Orders.SingleOrDefaultAsync(o => o.OrderId == orderId);
                    if (orderToUpdate == null)
                    {
                        return false; 
                    }
                    orderToUpdate.Status = orderStatus;
                    await db.SaveChangesAsync();
                    return true;
                }
            }));
    }
}