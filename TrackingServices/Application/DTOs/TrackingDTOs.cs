namespace TrackingServices.Application.DTOs
{
    public class TrackingDTOs
    {
       

    public record UpdateLocationRequest(
        Guid ShipmentId,
        double Latitude,
        double Longitude,
        string CurrentCity,
        string StatusMessage
    );

    public record LocationResponse(
        Guid DriverId,
        Guid? ShipmentId,
        double Latitude,
        double Longitude,
        string CurrentCity,
        string StatusMessage,
        DateTime RecordedAt
    );

    public record ShipmentStatusEvent(
        Guid ShipmentId,
        Guid DriverId,
        string CurrentCity,
        string StatusMessage,
        DateTime OccurredAt
    );
}
}
