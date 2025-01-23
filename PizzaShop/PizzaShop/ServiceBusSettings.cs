internal class ServiceBusSettings
{
    public string OrderQueueName { get; set; } = string.Empty;
    public string JobAcceptedQueueName { get; set; } = string.Empty;
    public string JobRejectedQueueName { get; set; } = string.Empty;
}