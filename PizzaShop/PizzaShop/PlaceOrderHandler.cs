using System.Threading.Channels;

namespace PizzaShop;

/// <summary>
/// When we have an order, we need to cook it and assign a courier
/// The courier is assigned randomly for purpose of this demo
/// </summary>
/// <param name="cookRequests">The channel for requests to cook - we produce to this</param>
/// <param name="deliveryRequests">The channel for requests to delivery - we produce to this</param>
/// <param name="couriers">The set of couriers that we can deliver to; we select at random from this</param>
internal class PlaceOrderHandler(Channel<CookRequest> cookRequests, Channel<DeliveryRequest> deliveryRequests, string[] couriers)
{
    public async Task<bool> HandleAsync(Order order, CancellationToken cancellationToken)
    {
        //should save the order
        //then raise accept/reject
        
        //kick off cook and assign courier tasks
       await cookRequests.Writer.WriteAsync(new CookRequest(order), cancellationToken);
       await deliveryRequests.Writer.WriteAsync(new DeliveryRequest(AssignCourier(), order), cancellationToken);
       
       return true;
    }

    private string AssignCourier()
    {
        return couriers[new Random().Next(0, couriers.Length)];
    }
}