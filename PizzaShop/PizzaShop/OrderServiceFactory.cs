using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PizzaShop;

public class OrderServiceFactory
{
    public static AsbMessagePumpService<Order> AddHostedOrderService(IServiceProvider serviceProvider)
    {
        var orderQueueName = serviceProvider.GetRequiredService<IOptions<ServiceBusSettings>>().Value.OrderQueueName;
        var couriers = serviceProvider.GetRequiredService<IOptions<CourierSettings>>().Value.Names ?? [];

        if (string.IsNullOrEmpty(orderQueueName))
        {
            throw new InvalidOperationException("ServiceBus:OrderQueueName and Courier:Names must be set in configuration");
        }

        var cookRequests = serviceProvider.GetRequiredService<Channel<CookRequest>>();
        var deliveryRequests = serviceProvider.GetRequiredService<Channel<DeliveryRequest>>();
        var courierStatusUpdates = serviceProvider.GetRequiredService<Channel<CourierStatusUpdate>>();
        var client = serviceProvider.GetRequiredService<ServiceBusClient>();
        var logger = serviceProvider.GetRequiredService<ILogger<AsbMessagePump<Order>>>();

        return new AsbMessagePumpService<Order>(
            client,
            orderQueueName,
            logger,
            message => JsonSerializer.Deserialize<Order>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid message"),
            async (order, token) => await new PlaceOrderHandler(cookRequests, deliveryRequests, couriers).HandleAsync(order, token));
    }
}