
namespace PizzaShop;

public class DeliveryManifest
{
    public DeliveryManifest(DeliveryRequest request)
    {
        OrderId = request.OrderId;
        DeliveryAddress = request.DeliveryAddress;
        PickupAddress = request.PickupAddress;
    }

    public int DeliveryManifestId { get; set; }
    public int OrderId { get; set; }
    public Address DeliveryAddress { get; set; }
    public Address PickupAddress { get; set; }
}