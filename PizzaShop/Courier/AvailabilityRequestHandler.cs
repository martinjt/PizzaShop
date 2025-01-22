using AsbGateway;

namespace Courier;

/// <summary>
/// Determines if a courier will accept or reject a job
/// </summary>
/// <param name="courierName">The name of this courier - used to address the matching queue</param>
/// <param name="acceptedProducer">The producer for an accept message</param>
/// <param name="rejectedProducer">The producer for a reject message</param>
public class AvailabilityRequestHandler(string courierName, AsbProducer<JobAccepted> acceptedProducer, AsbProducer<JobRejected> rejectedProducer)
{
    //use this to fake different paths
    private readonly bool _rejectJob = false;
    
    public async Task<bool> HandleAsync(DeliveryManifest job, CancellationToken token)
    {
        //we might want to save the job request if it would take too long for us to deliver
        // we will probably randomize that, so that couriers reject some jobs
        
        if (_rejectJob)
            await rejectedProducer.SendMessageAsync(courierName + "-job-rejected", new Message<JobRejected>(new JobRejected(job.OrderId, courierName)), token);
        else
            await acceptedProducer.SendMessageAsync(courierName + "-job-accepted", new Message<JobAccepted>(new JobAccepted(job.OrderId, courierName)), token);
        return true;
    }
}