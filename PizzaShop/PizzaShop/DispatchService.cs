using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

namespace PizzaShop;

public class DispatchService(Channel<DeliveryRequest> deliveryRequests) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await deliveryRequests.Reader.ReadAsync(stoppingToken);
            
            //request a delivery courier
        }
    }
}