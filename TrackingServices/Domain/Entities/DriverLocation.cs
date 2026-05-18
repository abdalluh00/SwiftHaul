namespace TrackingServices.Domain.Entities
{
    public class DriverLocation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DriverId { get; set; }
        public Guid? ShipmentId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CurrentCity { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
