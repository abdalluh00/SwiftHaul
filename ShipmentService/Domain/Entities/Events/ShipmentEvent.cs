namespace ShipmentService.Domain.Entities.Events
{

    public class ShipmentEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ShipmentId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public Guid? PerformedBy { get; set; } // Driver or Admin userId
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
