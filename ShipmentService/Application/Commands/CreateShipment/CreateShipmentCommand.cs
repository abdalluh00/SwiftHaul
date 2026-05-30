using SharedKernel.CQRS;
using ShipmentService.Application.Dtos;

namespace ShipmentService.Application.Commands.CreateShipment
{
    public record CreateShipmentCommand(
    Guid CustomerId,
    string CustomerRole,
    string PickupCity,
    string PickupAddress,
    string DeliveryCity,
    string DeliveryAddress,
    double WeightKg,
    string Description,
    bool IsInstant
) : ICommand<ShipmentResponse>;
}
