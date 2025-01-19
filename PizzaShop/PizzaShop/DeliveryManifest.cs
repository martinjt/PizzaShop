
namespace PizzaShop;

/// <summary>
/// An order that we have agreed to deliver
/// </summary>
/// <param name="request">The delivery request we are turning into a manifest</param>
public class DeliveryManifest(DeliveryRequest request)
{
    public string CourierId { get; set; } = request.CourierId;
    public int OrderId { get; set; } = request.OrderId;
    public Address DeliveryAddress { get; set; } = request.DeliveryAddress;
    public Address PickupAddress { get; set; } = request.PickupAddress;
}