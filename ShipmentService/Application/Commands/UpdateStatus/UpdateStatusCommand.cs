using SharedKernel.CQRS;

namespace ShipmentService.Application.Commands.UpdateStatus
{
    public record UpdateStatusCommand(
     Guid ShipmentId,
     Guid PerformedBy,
     int NewStatus,
     string Description,
     string City
 ) : ICommand;
}
