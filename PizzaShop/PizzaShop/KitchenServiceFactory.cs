using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace PizzaShop;

internal class KitchenServiceFactory
{
    public static KitchenService AddHostedKitchenService(IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<ServiceBusClient>();
        var cookRequests = serviceProvider.GetRequiredService<Channel<CookRequest>>();
        var courierStatusUpdates = serviceProvider.GetRequiredService<Channel<CourierStatusUpdate>>();

        var orderProducer = new AsbProducer<OrderReady>(
            client,
            message => new ServiceBusMessage(JsonSerializer.Serialize(message.Content)));

        var rejectedProducer = new AsbProducer<OrderRejected>(
            client,
            message => new ServiceBusMessage(JsonSerializer.Serialize(message.Content)));

        return new KitchenService(cookRequests, courierStatusUpdates, orderProducer, rejectedProducer);
    }

}