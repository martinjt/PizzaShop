using Courier;

public class DeliveryJobHandler
{
    public async Task<bool> Handle(DeliveryManifest job, CancellationToken token)
    {
        //should save the delivery
        //then raise accept/reject
        //kick off threads for courier tasks
        return true;
    }
}