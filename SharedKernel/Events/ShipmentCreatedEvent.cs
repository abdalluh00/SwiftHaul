
namespace SharedKernel.Events
{
    public record ShipmentCreatedEvent(
    Guid ShipmentId,
    string TrackingNumber,
    string PickupCity,
    string DeliveryCity,
    double WeightKg,
    Guid CustomerId,
    DateTime CreatedAt
);
}
