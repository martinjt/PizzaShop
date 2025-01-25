using Shared;
using StoreFrontCommon;

namespace StoreFrontWorker;

public class OrderStatusChange(int orderId, string newStatus) : TraceableRequest
{
    public DeliveryStatus NewStatus { get; set; } = Enum.Parse<DeliveryStatus>(newStatus);
    public int OrderId { get; set; } = orderId;
}