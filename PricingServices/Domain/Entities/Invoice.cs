namespace PricingServices.Domain.Entities
{

    public class Invoice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid ShipmentId { get; set; }
        public Guid CustomerId { get; set; }

        // Cities
        public string PickupCity { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;

        // Pricing breakdown
        public double WeightKg { get; set; }
        public double BasePricePerKg { get; set; }
        public double DistanceMultiplier { get; set; }
        public double TotalAmount { get; set; }

        // Status
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }

    public enum InvoiceStatus
    {
        Unpaid = 1,
        Paid = 2,
        Cancelled = 3
    }
}
