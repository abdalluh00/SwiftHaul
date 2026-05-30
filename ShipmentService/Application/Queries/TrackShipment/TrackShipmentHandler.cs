using SharedKernel.Common;
using SharedKernel.CQRS;
using ShipmentService.Application.Dtos;
using ShipmentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace ShipmentService.Application.Queries.TrackShipment
{
    public class TrackShipmentHandler
     : IQueryHandler<TrackShipmentQuery, TrackShipmentResponse>
    {
        private readonly AppDbContext _context;

        public TrackShipmentHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<TrackShipmentResponse>> Handle(
            TrackShipmentQuery request,
            CancellationToken cancellationToken)
        {
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.TrackingNumber == request.TrackingNumber,
                    cancellationToken);

            if (shipment is null)
                return Result.Failure<TrackShipmentResponse>(
                    Error.NotFound("Tracking number not found."));

            var events = await _context.ShipmentEvents
                .Where(e => e.ShipmentId == shipment.Id)
                .OrderBy(e => e.OccurredAt)
                .Select(e => new ShipmentHistoryResponse(
                    e.EventType,
                    e.Description,
                    e.City,
                    e.OccurredAt))
                .ToListAsync(cancellationToken);

            return Result.Success(new TrackShipmentResponse(
                shipment.TrackingNumber,
                shipment.CurrentStatus.ToString(),
                shipment.PickupCity,
                shipment.DeliveryCity,
                shipment.EstimatedDelivery,
                events));
        }
    }
}
