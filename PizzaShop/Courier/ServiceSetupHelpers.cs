using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Courier;

public static class ServiceSetupHelpers
{
    public static AsbMessagePumpService<DeliveryManifest> AddHostedAvailabilityRequestService(ConfigurationManager configurationManager, IServiceProvider serviceProvider)
    {
        var serviceBusClient = serviceProvider.GetRequiredService<ServiceBusClient>();
        var courierName = configurationManager.GetValue<string>("Courier:Name");

        if (string.IsNullOrEmpty(courierName))
        {
            throw new InvalidOperationException("Courier:Name must be set in configuration");
        }

        var logger = serviceProvider.GetRequiredService<ILogger<AsbMessagePump<DeliveryManifest>>>();

        var acceptedProducer = new AsbProducer<JobAccepted>(
            serviceBusClient,
            message => new ServiceBusMessage(JsonSerializer.Serialize(message.Content))
        );

        var rejectedProducer = new AsbProducer<JobRejected>(
            serviceBusClient,
            message => new ServiceBusMessage(JsonSerializer.Serialize(message.Content))
        );

        return new AsbMessagePumpService<DeliveryManifest>(
            serviceBusClient,
            courierName + "-availability",
            logger,
            message => JsonSerializer.Deserialize<DeliveryManifest>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid DeliveryManifest message"),
            async (job, token) => await new AvailabilityRequestHandler(courierName, acceptedProducer, rejectedProducer).HandleAsync(job, token));
    }

    public static AsbMessagePumpService<OrderReady> AddHostedOrderReadyService(ConfigurationManager configurationManager, IServiceProvider serviceProvider)
    {
        var courierName = configurationManager.GetValue<string>("Courier:Name");

        if (string.IsNullOrEmpty(courierName))
        {
            throw new InvalidOperationException("Courier:Name must be set in configuration");
        }

        var serviceBusClient = serviceProvider.GetRequiredService<ServiceBusClient>();
        var logger = serviceProvider.GetRequiredService<ILogger<AsbMessagePump<OrderReady>>>();
        var deliveryJobs = serviceProvider.GetRequiredService<Channel<OrderStatus>>();

        return new AsbMessagePumpService<OrderReady>(
            serviceBusClient,
            courierName + "-order-ready",
            logger,
            message => JsonSerializer.Deserialize<OrderReady>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid OrderReady message"),
            async (job, token) => await new OrderReadyHandler(courierName, deliveryJobs).HandleAsync(job, token));
    }

    public static DeliveryService AddHostedDispatcherService(ConfigurationManager configurationManager, IServiceProvider serviceProvider)
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