using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PizzaShop;

internal static class JobServiceFactory
{
    internal static AsbMessagePumpService<JobAccepted> CreateAccepted(string courierName, IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(courierName))
        {
            throw new InvalidOperationException("Courier:Name must be set in configuration");
        }
        var queueName = $"{courierName}-job-accepted";

        var courierStatusUpdates = serviceProvider.GetRequiredService<Channel<CourierStatusUpdate>>();
        var client = serviceProvider.GetRequiredService<ServiceBusClient>();
        var logger = serviceProvider.GetRequiredService<ILogger<AsbMessagePump<JobAccepted>>>();
        return new AsbMessagePumpService<JobAccepted>(
            client,
            queueName,
            logger,
            message => JsonSerializer.Deserialize<JobAccepted>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid message"),
            async (jobAccepted, token) => await new JobAcceptedHandler(courierStatusUpdates).HandleAsync(jobAccepted, token));
    }

    public static AsbMessagePumpService<JobRejected> CreateRejected(string courierName, IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(courierName))
        {
            throw new InvalidOperationException("Courier:Name must be set in configuration");
        }

        var queueName = $"{courierName}-job-rejected";

        var courierStatusUpdates = serviceProvider.GetRequiredService<Channel<CourierStatusUpdate>>();
        var client = serviceProvider.GetRequiredService<ServiceBusClient>();
        var logger = serviceProvider.GetRequiredService<ILogger<AsbMessagePump<JobRejected>>>();
        return new AsbMessagePumpService<JobRejected>(
            client,
            queueName,
            logger,
            message => JsonSerializer.Deserialize<JobRejected>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid message"),
            async (jobRejected, token) => await new JobRejectedHandler(courierStatusUpdates).HandleAsync(jobRejected, token));
    }
}