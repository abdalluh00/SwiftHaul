using Microsoft.EntityFrameworkCore;
using SharedKernel.Common;
using SharedKernel.CQRS;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.Entities.Events;
using ShipmentService.Infrastructure.Data;

namespace ShipmentService.Application.Commands.UpdateStatus
{
    public class UpdateStatusHandler : ICommandHandler<UpdateStatusCommand>
    {
        private readonly AppDbContext _context;

        public UpdateStatusHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(
            UpdateStatusCommand request,
            CancellationToken cancellationToken)
        {
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.Id == request.ShipmentId, cancellationToken);

            if (shipment is null)
                return Result.Failure(Error.NotFound("Shipment not found."));

            var newStatus = (ShipmentStatus)request.NewStatus;

            if (!IsValidTransition(shipment.CurrentStatus, newStatus))
                return Result.Failure(Error.Validation(
                    $"Cannot transition from {shipment.CurrentStatus} to {newStatus}."));

            shipment.CurrentStatus = newStatus;

            _context.ShipmentEvents.Add(new ShipmentEvent
            {
                ShipmentId = shipment.Id,
                EventType = newStatus.ToString(),
                Description = request.Description,
                City = request.City,
                PerformedBy = request.PerformedBy
            });

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        private static bool IsValidTransition(ShipmentStatus current, ShipmentStatus next) =>
            (current, next) switch
            {
                (ShipmentStatus.Created, ShipmentStatus.PickedUp) => true,
                (ShipmentStatus.PickedUp, ShipmentStatus.InTransit) => true,
                (ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery) => true,
                (ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered) => true,
                (ShipmentStatus.OutForDelivery, ShipmentStatus.Failed) => true,
                (ShipmentStatus.Failed, ShipmentStatus.Returned) => true,
                (ShipmentStatus.Created, ShipmentStatus.Cancelled) => true,
                _ => false
            };
    }
}
