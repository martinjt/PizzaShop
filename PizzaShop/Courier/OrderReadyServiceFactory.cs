using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Courier;

internal static class OrderReadyServiceFactory
{
    public static AsbMessagePumpService<OrderReady> Create(ConfigurationManager configurationManager, IServiceProvider serviceProvider)
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
}