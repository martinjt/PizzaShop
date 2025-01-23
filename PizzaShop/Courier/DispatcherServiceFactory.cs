using System.Threading.Channels;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Courier;

internal static class DispatcherServiceFactory
{
    public static DeliveryService Create(ConfigurationManager configurationManager, IServiceProvider serviceProvider)
    {
        var courierName = configurationManager.GetValue<string>("Courier:Name");

        if (string.IsNullOrEmpty(courierName))
        {
            throw new InvalidOperationException("ServiceBus:DeliveryManifestQueueName must be set in configuration");
        }

        var orderStatusProducer = serviceProvider.GetRequiredService<IProducer<int, string>>();
        var deliveryJobs = serviceProvider.GetRequiredService<Channel<OrderStatus>>();

        return new DeliveryService(courierName + "-order-status", deliveryJobs, orderStatusProducer);

    }
}