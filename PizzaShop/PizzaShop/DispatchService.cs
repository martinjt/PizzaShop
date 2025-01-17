using System.Threading.Channels;
using AsbGateway;
using Microsoft.Extensions.Hosting;

namespace PizzaShop;

public class DispatchService(Channel<DeliveryRequest> deliveryRequests, AsbProducer<DeliveryManifest> producer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await deliveryRequests.Reader.ReadAsync(stoppingToken);

            await producer.SendMessageAsync(new Message<DeliveryManifest>(new DeliveryManifest(request)), stoppingToken);
        }
    }
}