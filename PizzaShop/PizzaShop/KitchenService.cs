using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

namespace PizzaShop;

public class KitchenService(Channel<CookRequest> cookRequests) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await cookRequests.Reader.ReadAsync(stoppingToken);
            
            //cook the pizza
            await Task.Delay(5000, stoppingToken); //simulate cooking time
            
            //signal that the pizza is cooked
        }
    }
}