namespace ShipmentService.Domain.Entities
{
    public class Shipment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TrackingNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? DriverId { get; set; }

        // Pickup
        public string PickupCity { get; set; } = string.Empty;
        public string PickupAddress { get; set; } = string.Empty;

        // Delivery
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;

        // Package details
        public double WeightKg { get; set; }
        public string Description { get; set; } = string.Empty;

        // Status
        public ShipmentStatus CurrentStatus { get; set; } = ShipmentStatus.Created;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EstimatedDelivery { get; set; }

        // Booking type
        public bool IsInstant { get; set; } = false;
    }

    public enum ShipmentStatus
    {
        Created = 1,
        PickedUp = 2,
        InTransit = 3,
        OutForDelivery = 4,
        Delivered = 5,
        Failed = 6,
        Returned = 7,
        Cancelled = 8
    }
}
