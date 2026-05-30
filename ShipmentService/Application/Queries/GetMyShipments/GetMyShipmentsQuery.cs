using SharedKernel.CQRS;
using ShipmentService.Application.Dtos;

namespace ShipmentService.Application.Queries.GetMyShipments
{
    public record GetMyShipmentsQuery(Guid CustomerId) : IQuery<List<ShipmentResponse>>;
}
