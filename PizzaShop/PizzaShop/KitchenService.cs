using System.Threading.Channels;
using AsbGateway;
using Microsoft.Extensions.Hosting;

namespace PizzaShop;

public class KitchenService(
    Channel<CookRequest> cookRequests, 
    Channel<CourierStatusUpdate> courierStatusUpdates, 
    AsbProducer<OrderReady> readyProducer,
    AsbProducer<OrderRejected> rejectedProducer
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var request = await cookRequests.Reader.ReadAsync(stoppingToken);
            //TODO: Get the Order out of the Db via EF
            var order = new Order();
            
            //cook the pizza
            await Task.Delay(5000, stoppingToken); //simulate cooking time
            
            //TODO: Update the Order in the Db via EF
            
            //await the courier to accept the order -- assume the first available courier will accept
            var courierStatusUpdate = await courierStatusUpdates.Reader.ReadAsync(stoppingToken);
            if (courierStatusUpdate.Status == CourierStatus.Accepted)
                await readyProducer.SendMessageAsync(new Message<OrderReady>(new OrderReady(order, courierStatusUpdate.CourierId)), stoppingToken);
            else if (courierStatusUpdate.Status == CourierStatus.Rejected)
                await rejectedProducer.SendMessageAsync(new Message<OrderRejected>(new OrderRejected(order.OrderId)), stoppingToken);    
            else
                throw new InvalidOperationException("Invalid courier status");
            
        }
    }
}