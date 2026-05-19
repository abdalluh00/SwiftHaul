namespace PricingServices.Application.DTOs
{
    public class PricingDTOs
    {
       

    public record CreateInvoiceRequest(
        Guid ShipmentId,
        Guid CustomerId,
        string PickupCity,
        string DeliveryCity,
        double WeightKg
    );

    public record InvoiceResponse(
        Guid Id,
        string InvoiceNumber,
        Guid ShipmentId,
        string PickupCity,
        string DeliveryCity,
        double WeightKg,
        double BasePricePerKg,
        double DistanceMultiplier,
        double TotalAmount,
        string Status,
        DateTime CreatedAt
    );

    public record PriceEstimateRequest(
        string PickupCity,
        string DeliveryCity,
        double WeightKg
    );

    public record PriceEstimateResponse(
        string PickupCity,
        string DeliveryCity,
        double WeightKg,
        double BasePricePerKg,
        double DistanceMultiplier,
        double EstimatedTotal
    );
}
}
