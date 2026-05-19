namespace NotificationServices.Application.DTOs
{
    public record ShipmentStatusEvent(
     Guid ShipmentId,
     Guid DriverId,
     string CurrentCity,
     string StatusMessage,
     DateTime OccurredAt
 );
}
