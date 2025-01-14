using System.Threading.Channels;

namespace PizzaShop;

public class PlaceOrderHandler(Channel<CookRequest> cookRequests, Channel<DeliveryRequest> deliveryRequests)
{
    public async Task<bool> HandleAsync(Order order, CancellationToken cancellationToken)
    {
        //should save the order
        //then raise accept/reject
        
        //kick off cook and assign courier tasks
       await cookRequests.Writer.WriteAsync(new CookRequest(order), cancellationToken);
       await deliveryRequests.Writer.WriteAsync(new DeliveryRequest(order), cancellationToken);
       
       return true;
    }
}