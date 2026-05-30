using SharedKernel.CQRS;
using ShipmentService.Application.Dtos;

namespace ShipmentService.Application.Queries.TrackShipment
{
    public record TrackShipmentQuery(string TrackingNumber) : IQuery<TrackShipmentResponse>;

}
