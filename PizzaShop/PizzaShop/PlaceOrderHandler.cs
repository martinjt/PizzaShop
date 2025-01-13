namespace PizzaShop;

public class PlaceOrderHandler
{
    public PlaceOrderHandler()
    {
    }
    
    public async Task<bool> Handle(Order order, CancellationToken cancellationToken)
    {
        //should save the order
        //then raise accept/reject
        //kick off threads for cook and assign courier tasks
        return true;
    }
}