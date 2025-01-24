using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace StoreFront;

internal class AfterOrderService(PizzaShopDb? db)
{
    public async Task<bool> HandleAsync(int key, string value)
    {
        var orderId = key;
        var orderStatus = Enum.Parse<DeliveryStatus>(value);
        
        Activity.Current?.AddEvent(new ActivityEvent("OrderStatusChange", tags: new ActivityTagsCollection
        {
            ["OrderId"] = orderId,
            ["OrderStatus"] = orderStatus
        }));
        
        if (db is null) throw new InvalidOperationException("No  EF Context");
                                                                        
        var orderToUpdate = await db.Orders.SingleOrDefaultAsync(o => o.OrderId == orderId);
        if (orderToUpdate == null)
        {
            return false;
        }

        orderToUpdate.Status = orderStatus;
        await db.SaveChangesAsync();
        return true;
    } 
}