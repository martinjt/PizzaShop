using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PizzaShop;

//our pizza shop has internal channels for its orchestration
// -- cookrequests are sent to the kitchen
// -- deliveryrequests are sent to the dispatch service
// - courierstatusupdates are sent to the kitchen

var cookRequests = Channel.CreateBounded<CookRequest>(10);
var deliveryRequests = Channel.CreateBounded<DeliveryRequest>(10);
var courierStatusUpdates = Channel.CreateBounded<CourierStatusUpdate>(10);

var hostBuilder = new HostBuilder()
    .ConfigureHostConfiguration((config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.SetBasePath(Environment.CurrentDirectory);
        config.AddJsonFile("appsettings.json", optional: false);
        config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        //we use multiple pumps, because we don't want all channels to become unresponsive because one gets busy!!! In practice, we would
        //want competing consumers here
        services.AddHostedService<AsbMessagePumpService<Order>>(serviceProvider => AddHostedOrderService(hostContext, serviceProvider));
        services.AddHostedService<AsbMessagePumpService<JobAccepted>>(serviceProvider => AddHostedJobAcceptedService(hostContext, serviceProvider));
        services.AddHostedService<AsbMessagePumpService<JobRejected>>(serviceProvider => AddHostedOrderRejectedService(hostContext, serviceProvider)); 
        
        //We use channels for our internal pipeline. Channels let us easily wait on work without synchronization primitives
        services.AddHostedService<KitchenService>(_ => AddHostedKitchenService(hostContext));
        services.AddHostedService<DispatchService>(_ => AddHostedDispatcherService(hostContext));
    });

await hostBuilder.Build().RunAsync();

AsbMessagePumpService<Order> AddHostedOrderService(HostBuilderContext hostBuilderContext, IServiceProvider serviceProvider)
{
    var connectionString = hostBuilderContext.Configuration["ServiceBus:ConnectionString"];
    var queueName = hostBuilderContext.Configuration["ServiceBus:OrderQueueName"];
            
    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queueName))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString and ServiceBus:OrderQueueName must be set in configuration");
    }
            
    var client = new ServiceBusClient(connectionString);
    var logger = serviceProvider.GetRequiredService<ILogger<AsbMessagePump<Order>>>();
    return new AsbMessagePumpService<Order>(
        client, 
        queueName, 
        logger,
        message => JsonSerializer.Deserialize<Order>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid message"), 
        async (order, token) => await new PlaceOrderHandler(cookRequests, deliveryRequests).HandleAsync(order, token));
}

AsbMessagePumpService<JobAccepted> AddHostedJobAcceptedService(HostBuilderContext hostBuilderContext, IServiceProvider serviceProvider1)
{
    var connectionString = hostBuilderContext.Configuration["ServiceBus:ConnectionString"];
    var queueName = hostBuilderContext.Configuration["ServiceBus:JobAcceptedQueueName"];
            
    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queueName))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString and ServiceBus:JobAcceptedQueueName must be set in configuration");
    }
            
    var client = new ServiceBusClient(connectionString);
    var logger = serviceProvider1.GetRequiredService<ILogger<AsbMessagePump<JobAccepted>>>();
    return new AsbMessagePumpService<JobAccepted>(
        client, 
        queueName, 
        logger,
        message => JsonSerializer.Deserialize<JobAccepted>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid message"), 
        async (jobAccepted, token) => await new JobAcceptedHandler(courierStatusUpdates).HandleAsync(jobAccepted, token));
}

AsbMessagePumpService<JobRejected> AddHostedOrderRejectedService(HostBuilderContext hostBuilderContext, IServiceProvider serviceProvider2)
{
    var connectionString = hostBuilderContext.Configuration["ServiceBus:ConnectionString"];
    var queueName = hostBuilderContext.Configuration["ServiceBus:JobRejectedQueueName"];
            
    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queueName))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString and ServiceBus:JobRejectedQueueName must be set in configuration");
    }
            
    var client = new ServiceBusClient(connectionString);
    var logger = serviceProvider2.GetRequiredService<ILogger<AsbMessagePump<JobRejected>>>();
    return new AsbMessagePumpService<JobRejected>(
        client, 
        queueName, 
        logger,
        message => JsonSerializer.Deserialize<JobRejected>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid message"), 
        async (jobRejected, token) => await new JobRejectedHandler(courierStatusUpdates).HandleAsync(jobRejected, token));
}

KitchenService AddHostedKitchenService(HostBuilderContext hostBuilderContext)
{
    var connectionString = hostBuilderContext.Configuration["ServiceBus:ConnectionString"];
    var orderReadyQueueName = hostBuilderContext.Configuration["ServiceBus:OrderReadyQueueName"];
    var orderRejectedQueueName = hostBuilderContext.Configuration["ServiceBus:OrderRejectedQueueName"];
            
    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(orderReadyQueueName) || string.IsNullOrEmpty(orderRejectedQueueName))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString and ServiceBus:OrderRejectedQueueName must be set in configuration");
    }
            
    var orderProducer = new AsbProducer<OrderReady>(
        new ServiceBusClient(connectionString),
        orderReadyQueueName,
        message => new ServiceBusMessage(JsonSerializer.Serialize(message)));
    
    var rejectedProducer = new AsbProducer<OrderRejected>(
        new ServiceBusClient(connectionString),
        orderRejectedQueueName,
        message => new ServiceBusMessage(JsonSerializer.Serialize(message)));
            
    return new KitchenService(cookRequests, courierStatusUpdates, orderProducer, rejectedProducer);
}

DispatchService AddHostedDispatcherService(HostBuilderContext hostBuilderContext)
{
    var connectionString = hostBuilderContext.Configuration["ServiceBus:ConnectionString"];
    var deliverManifestQueueName = hostBuilderContext.Configuration["ServiceBus:OrderReadyQueueName"];

    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(deliverManifestQueueName))
    {
        throw new InvalidOperationException("ServiceBus: ConnectionString and ServiceBus:DeliveryManifestQueueName must be set in configuration");
    }

    var deliveryManifestProducer = new AsbProducer<DeliveryManifest>(
        new ServiceBusClient(connectionString),
        deliverManifestQueueName,
        message => new ServiceBusMessage(JsonSerializer.Serialize(message)));

    return new DispatchService(deliveryRequests, deliveryManifestProducer);

}