using SharedKernel.Common;
using SharedKernel.CQRS;
using ShipmentService.Application.Dtos;
using ShipmentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ShipmentService.Application.Queries.GetMyShipments
{
    public class GetMyShipmentsHandler
     : IQueryHandler<GetMyShipmentsQuery, List<ShipmentResponse>>
    {
        private readonly AppDbContext _context;

        public GetMyShipmentsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ShipmentResponse>>> Handle(
            GetMyShipmentsQuery request,
            CancellationToken cancellationToken)
        {
            var shipments = await _context.Shipments
                .Where(s => s.CustomerId == request.CustomerId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ShipmentResponse(
                    s.Id,
                    s.TrackingNumber,
                    s.PickupCity,
                    s.DeliveryCity,
                    s.CurrentStatus.ToString(),
                    s.CreatedAt,
                    s.EstimatedDelivery))
                .ToListAsync(cancellationToken);

            return Result.Success(shipments);
        }
    }
}
