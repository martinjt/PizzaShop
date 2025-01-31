using System.Diagnostics;
using System.Threading.Channels;
using AsbGateway;
using Microsoft.Extensions.Hosting;
using Shared;

namespace PizzaShop;

/// <summary>
/// A background service that listens to cook requests from the OrderService, cooks them and notifies the assigned courier when it's ready
/// </summary>
/// <param name="cookRequests">A queue of requests to cook pizza</param>
/// <param name="courierStatusUpdates">A queue of responses from the courier to pick up the order</param>
/// <param name="readyProducer"></param>
/// <param name="rejectedProducer"></param>
internal class KitchenService(
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
            Activity? activity = null;
            try
            {
                var request = await cookRequests.Reader.ReadAsync(stoppingToken);
                activity = request.StartNewSpanFromRequest();
                
                activity?.SetTag("pizzashop.pizza.total_count", request.Pizzas.Count());
                activity?.SetTag("pizzashop.order.id", request.OrderId);

                var cookTasks = request.Pizzas.Select(CookPizza);

                await Task.WhenAll(cookTasks);

                //await the courier to accept the order -- assume the first available courier will accept
                var courierStatusUpdate = await courierStatusUpdates.Reader.ReadAsync(stoppingToken);
                if (courierStatusUpdate.Status == CourierStatus.Accepted)
                    await readyProducer.SendMessageAsync(
                        courierStatusUpdate.CourierId + "-order-ready",
                        new Message<OrderReady>(new OrderReady(request.OrderId, courierStatusUpdate.CourierId)), stoppingToken);

                //NOTE: we don't handle the case where the courier rejects the order, this is deliberate for the purpose of this demo
                //in principle we would need to handle this case and reassign the order to another courier
            }
            catch (OperationCanceledException oce)
            {
                activity?.AddException(oce);
                Debug.WriteLine(oce.Message);
            }
            finally{
                activity?.Dispose();
            }
        }
    }

    private async Task CookPizza(Pizza pizza)
    {
        var cooktime = pizza.Size switch
        {
            PizzaSize.Small => 1000,
            PizzaSize.Medium => 1500,
            PizzaSize.Large => 2000,
            PizzaSize.ExtraLarge => 3500,
            _ => throw new NotImplementedException()
        };

        using var activity = DiagnosticConfig.Source.StartActivity("Cook Pizza", ActivityKind.Internal, null, tags: [
            new("pizzashop.pizza.size", pizza.Size),
            new("pizzashop.order.id", pizza.OrderId)
        ] );
        await Task.Delay(cooktime);

    }
}