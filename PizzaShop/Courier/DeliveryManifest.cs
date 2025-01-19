namespace Courier;

/// <summary>
/// What is the work that we are being asked to carry out
/// </summary>
public class DeliveryManifest
{
    public int DeliveryManifestId { get; set; }
    public int OrderId { get; set; }
    public Address DeliveryAddress { get; set; } = new Address();
    public Address PickupAddress { get; set; } = new Address();
}