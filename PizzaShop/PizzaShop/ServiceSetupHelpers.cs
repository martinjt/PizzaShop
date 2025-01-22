using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PizzaShop;

public static class ServiceSetupHelpers
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

    public static AsbMessagePumpService<JobAccepted> AddHostedJobAcceptedService(string courierName, IServiceProvider serviceProvider)
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

    public static AsbMessagePumpService<JobRejected> AddHostedJobRejectedService(string courierName, IServiceProvider serviceProvider)
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