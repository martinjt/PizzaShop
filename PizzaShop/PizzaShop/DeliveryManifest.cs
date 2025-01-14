
namespace PizzaShop;

public class DeliveryManifest
{
    public int DeliveryManifestId { get; set; }
    public int OrderId { get; set; }
    public Address DeliveryAddress { get; set; } = new Address();
    public Address PickupAddress { get; set; } = new Address();
}