using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PizzaShop;

//our pizza shop has internal channels for its orchestration
// -- cookrequests are sent to the kitchen
// -- deliveryrequests are sent to the dispatch service
// -- courierstatusupdates are sent to the kitchen

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
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<CourierSettings>(hostContext.Configuration.GetSection("CourierSettings"));
        services.Configure<ServiceBusSettings>(hostContext.Configuration.GetSection("ServiceBus"));
        //we use multiple pumps, because we don't want all channels to become unresponsive because one gets busy!!! In practice, we would
        //want competing consumers here
        services.AddHostedService<AsbMessagePumpService<Order>>(AddHostedOrderService);

        var courierSettings = hostContext.Configuration.GetSection("CourierSettings").Get<CourierSettings>();
        
        //we have distinct queues for job accepted and job rejected to listen to each courier - all post to the same channel#
        foreach (var name in courierSettings.Names)
        {
            services.AddHostedService<AsbMessagePumpService<JobAccepted>>(serviceProvider => AddHostedJobAcceptedService(name + "-Job-Accepted", serviceProvider));
            services.AddHostedService<AsbMessagePumpService<JobRejected>>(serviceProvider => AddHostedOrderRejectedService(name + "-Job-Rejected", serviceProvider));
        }
        
        //We use channels for our internal pipeline. Channels let us easily wait on work without synchronization primitives
        services.AddHostedService<KitchenService>(AddHostedKitchenService);
        services.AddHostedService<DispatchService>(AddHostedDispatcherService);
    });

await hostBuilder.Build().RunAsync();

AsbMessagePumpService<Order> AddHostedOrderService(IServiceProvider serviceProvider)
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("pizza-shop-service-bus");
    var queueName = serviceProvider.GetRequiredService<IOptions<ServiceBusSettings>>().Value.OrderQueueName;
    var couriers = serviceProvider.GetRequiredService<IOptions<CourierSettings>>().Value.Names;
            
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
        async (order, token) => await new PlaceOrderHandler(cookRequests, deliveryRequests, couriers).HandleAsync(order, token));
}

AsbMessagePumpService<JobAccepted> AddHostedJobAcceptedService(string queueName, IServiceProvider serviceProvider)
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("pizza-shop-service-bus");
            
    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queueName))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString and ServiceBus:JobAcceptedQueueName must be set in configuration");
    }
            
    var client = new ServiceBusClient(connectionString);
    var logger = serviceProvider.GetRequiredService<ILogger<AsbMessagePump<JobAccepted>>>();
    return new AsbMessagePumpService<JobAccepted>(
        client, 
        queueName, 
        logger,
        message => JsonSerializer.Deserialize<JobAccepted>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid message"), 
        async (jobAccepted, token) => await new JobAcceptedHandler(courierStatusUpdates).HandleAsync(jobAccepted, token));
}

AsbMessagePumpService<JobRejected> AddHostedOrderRejectedService(string queueName, IServiceProvider serviceProvider)
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("pizza-shop-service-bus");
            
    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queueName))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString and ServiceBus:JobRejectedQueueName must be set in configuration");
    }
            
    var client = new ServiceBusClient(connectionString);
    var logger = serviceProvider.GetRequiredService<ILogger<AsbMessagePump<JobRejected>>>();
    return new AsbMessagePumpService<JobRejected>(
        client, 
        queueName, 
        logger,
        message => JsonSerializer.Deserialize<JobRejected>(message.Body.ToString()) ?? throw new InvalidOperationException("Invalid message"), 
        async (jobRejected, token) => await new JobRejectedHandler(courierStatusUpdates).HandleAsync(jobRejected, token));
}

KitchenService AddHostedKitchenService(IServiceProvider serviceProvider)
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString("pizza-shop-service-bus");
            
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString and ServiceBus:OrderRejectedQueueName must be set in configuration");
    }
            
    var orderProducer = new AsbProducer<OrderReady>(
        new ServiceBusClient(connectionString),
        message => new ServiceBusMessage(JsonSerializer.Serialize(message)));
    
    var rejectedProducer = new AsbProducer<OrderRejected>(
        new ServiceBusClient(connectionString),
        message => new ServiceBusMessage(JsonSerializer.Serialize(message)));
            
    return new KitchenService(cookRequests, courierStatusUpdates, orderProducer, rejectedProducer);
}

DispatchService AddHostedDispatcherService(IServiceProvider serviceProvider)
{
    var connectionString = serviceProvider.GetRequiredService<IConfiguration>()
        .GetConnectionString("pizza-shop-service-bus");
    var deliverManifestQueueName = serviceProvider
        .GetRequiredService<IOptions<ServiceBusSettings>>()
        .Value.DeliveryManifestQueueName;

    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(deliverManifestQueueName))
    {
        throw new InvalidOperationException("ServiceBus: ConnectionString and ServiceBus:DeliveryManifestQueueName must be set in configuration");
    }

    var deliveryManifestProducer = new AsbProducer<DeliveryManifest>(
        new ServiceBusClient(connectionString),
        message => new ServiceBusMessage(JsonSerializer.Serialize(message)));

    return new DispatchService(deliveryRequests, deliveryManifestProducer);

}

public class CourierSettings
{
    public string[] Names { get; set; }
}

public class ServiceBusSettings
{
    public string OrderQueueName { get; set; }
    public string DeliveryManifestQueueName { get; set; }
}