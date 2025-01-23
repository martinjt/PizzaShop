using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

namespace PizzaShop;

internal class DispatcherServiceFactory
{
    
    public static DispatchService AddHostedDispatcherService(IServiceProvider serviceProvider)
    {
        var client = serviceProvider.GetRequiredService<ServiceBusClient>();
        var deliveryRequests = serviceProvider.GetRequiredService<Channel<DeliveryRequest>>();

        var deliveryManifestProducer = new AsbProducer<DeliveryManifest>(
            client,
            message => new ServiceBusMessage(JsonSerializer.Serialize(message.Content)));

        return new DispatchService(deliveryRequests, deliveryManifestProducer);

    }
}