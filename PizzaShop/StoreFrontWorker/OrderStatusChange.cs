using Shared;
using StoreFrontCommon;

namespace StoreFrontWorker;

public class OrderStatusChange() : TraceableRequest
{
    public string? CourierId { get; set; }
    public int OrderId { get; set; }
    public DateTimeOffset ETA { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
}