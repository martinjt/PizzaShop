using System.Text.Json;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Courier;

internal static class AvailabilityServiceFactory
{
    public static AsbMessagePumpService<DeliveryManifest> Create(ConfigurationManager configurationManager, IServiceProvider serviceProvider)
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

}