using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Shared;
using StoreFrontCommon;

namespace StoreFrontWorker;

internal class AfterOrderService(IDbContextFactory<PizzaShopDb> dbContextFactory)
{
    public async Task<bool> HandleAsync(OrderStatusChange orderStatusChange)
    {
        orderStatusChange.AddCurrentTraceContext();
        
        Activity.Current?.AddEvent(new ActivityEvent("OrderStatusChange", tags: new ActivityTagsCollection
        {
            ["OrderId"] = orderStatusChange.OrderId,
            ["OrderStatus"] = orderStatusChange.NewStatus
        }));
        
        if (dbContextFactory is null) throw new InvalidOperationException("No  EF Context Factory registered");

        await using var db = await dbContextFactory.CreateDbContextAsync() ;
                                                                        
        var orderToUpdate = await db.Orders.SingleOrDefaultAsync(o => o.OrderId == orderStatusChange.OrderId);
        if (orderToUpdate == null)
            return false;

        orderToUpdate.Status = orderStatusChange.NewStatus;
        await db.SaveChangesAsync();
        return true;
    }
}