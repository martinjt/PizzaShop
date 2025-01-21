using System.Threading.Channels;
using AsbGateway;
using Microsoft.Extensions.Hosting;

namespace PizzaShop;

/// <summary>
/// A background service that listens to cook requests from the OrderService, cooks them and notifies the assigned courier when it's ready
/// </summary>
/// <param name="cookRequests">A queue of requests to cook pizza</param>
/// <param name="courierStatusUpdates">A queue of responses from the courier to pick up the order</param>
/// <param name="readyProducer"></param>
/// <param name="rejectedProducer"></param>
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
            
            //cook the pizza
            await Task.Delay(5000, stoppingToken); //simulate cooking time
            
            //await the courier to accept the order -- assume the first available courier will accept
            var courierStatusUpdate = await courierStatusUpdates.Reader.ReadAsync(stoppingToken);
            if (courierStatusUpdate.Status == CourierStatus.Accepted)
                await readyProducer.SendMessageAsync(
                    courierStatusUpdate.CourierId + "-order-ready", 
                    new Message<OrderReady>(new OrderReady(request.OrderId, courierStatusUpdate.CourierId)), stoppingToken);
             
            //NOTE: we don't handle the case where the courier rejects the order, this is deliberate for the purpose of this demo
            //in principle we would need to handle this case and reassign the order to another courier
        }
    }
}