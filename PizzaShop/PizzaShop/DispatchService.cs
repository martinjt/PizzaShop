using System.Threading.Channels;
using AsbGateway;
using Microsoft.Extensions.Hosting;
using Shared;

namespace PizzaShop;

/// <summary>
/// A background service that listens to delivery requests from the KitchenService and sends them to the courier
/// </summary>
/// <param name="deliveryRequests">The channel of deliveryRequests from the Kitchen</param>
/// <param name="producer">The ASB producer for delivery requests</param>
internal class DispatchService(Channel<DeliveryRequest> deliveryRequests, AsbProducer<DeliveryManifest> producer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await deliveryRequests.Reader.ReadAsync(stoppingToken);
            using var activity = request.StartNewSpanFromRequest();

            await producer.SendMessageAsync(request.CourierId + "-availability", new Message<DeliveryManifest>(new DeliveryManifest(request)), stoppingToken);
        }
    }
}