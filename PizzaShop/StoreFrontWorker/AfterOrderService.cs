using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Shared;
using StoreFrontCommon;

namespace StoreFrontWorker;

internal class AfterOrderService(IDbContextFactory<PizzaShopDb> dbContextFactory)
{
    public bool Handle(OrderStatusChange orderStatusChange)
    {
        orderStatusChange.AddCurrentTraceContext();
        
        if (dbContextFactory is null) throw new InvalidOperationException("No  EF Context Factory registered");

        using var db = dbContextFactory.CreateDbContext() ;
                                                                        
        var orderToUpdate = db.Orders.SingleOrDefault(o => o.OrderId == orderStatusChange.OrderId);
        if (orderToUpdate == null)
            return false ;

        orderToUpdate.Status = orderStatusChange.NewStatus;
        db.SaveChanges();
        return true;
    }
}