namespace ShipmentService.Application.Dtos
{

    public record CreateShipmentRequest(
        string PickupCity,
        string PickupAddress,
        string DeliveryCity,
        string DeliveryAddress,
        double WeightKg,
        string Description,
        bool IsInstant
    );

    public record UpdateStatusRequest(
        Guid ShipmentId,
        int NewStatus, // use ShipmentStatus enum value
        string Description,
        string City
    );

    public record ShipmentResponse(
        Guid Id,
        string TrackingNumber,
        string PickupCity,
        string DeliveryCity,
        string CurrentStatus,
        DateTime CreatedAt,
        DateTime? EstimatedDelivery
    );

    public record ShipmentHistoryResponse(
        string EventType,
        string Description,
        string City,
        DateTime OccurredAt
    );

    public record TrackShipmentResponse(
        string TrackingNumber,
        string CurrentStatus,
        string PickupCity,
        string DeliveryCity,
        DateTime? EstimatedDelivery,
        List<ShipmentHistoryResponse> History
    );
}
