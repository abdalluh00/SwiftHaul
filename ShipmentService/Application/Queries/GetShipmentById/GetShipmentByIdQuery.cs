using SharedKernel.CQRS;
using ShipmentService.Application.Dtos;

namespace ShipmentService.Application.Queries.GetShipmentById
{
    public record GetShipmentByIdQuery(Guid ShipmentId) : IQuery<ShipmentResponse>;
}
