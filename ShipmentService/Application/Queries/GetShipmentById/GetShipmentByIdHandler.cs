using Microsoft.EntityFrameworkCore;
using SharedKernel.Common;
using SharedKernel.CQRS;
using ShipmentService.Application.Dtos;
using ShipmentService.Infrastructure.Data;

namespace ShipmentService.Application.Queries.GetShipmentById
{
    public class GetShipmentByIdHandler
    : IQueryHandler<GetShipmentByIdQuery, ShipmentResponse>
    {
        private readonly AppDbContext _context;

        public GetShipmentByIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<ShipmentResponse>> Handle(
            GetShipmentByIdQuery request,
            CancellationToken cancellationToken)
        {
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

            if (shipment is null)
                return Result.Failure<ShipmentResponse>(
                    Error.NotFound("Shipment not found."));

            return Result.Success(new ShipmentResponse(
                shipment.Id,
                shipment.TrackingNumber,
                shipment.PickupCity,
                shipment.DeliveryCity,
                shipment.CurrentStatus.ToString(),
                shipment.CreatedAt,
                shipment.EstimatedDelivery));
        }
    }
}
