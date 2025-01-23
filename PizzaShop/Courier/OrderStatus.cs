using Shared;

namespace Courier;

/// <summary>
/// What is the status of our order?
/// </summary>
public class OrderStatus : TraceableRequest
{
    public string? CourierId { get; set; }
    public int OrderId { get; set; }
    public DateTimeOffset ETA { get; set; }
    public string? Status { get; set; }    
}