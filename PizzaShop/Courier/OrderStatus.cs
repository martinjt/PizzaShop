namespace KafkaGateway;

public class OrderStatus
{
    public string OrderId { get; set; }
    public DateTimeOffset ETA { get; set; }
    public string Status { get; set; }    
}