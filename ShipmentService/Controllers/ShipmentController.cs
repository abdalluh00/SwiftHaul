using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Extensions;
using ShipmentService.Application.Commands.CreateShipment;
using ShipmentService.Application.Commands.UpdateStatus;
using ShipmentService.Application.Queries.GetMyShipments;
using ShipmentService.Application.Queries.GetShipmentById;
using ShipmentService.Application.Queries.TrackShipment;
using System.Security.Claims;

namespace ShipmentService.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ShipmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Customer,Company")]
        public async Task<IActionResult> CreateShipment(
            [FromBody] Application.Dtos.CreateShipmentRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;

            var command = new CreateShipmentCommand(
                userId,
                userRole,
                request.PickupCity,
                request.PickupAddress,
                request.DeliveryCity,
                request.DeliveryAddress,
                request.WeightKg,
                request.Description,
                request.IsInstant);

            var result = await _mediator.Send(command);
            return this.ToActionResult(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] Application.Dtos.UpdateStatusRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var command = new UpdateStatusCommand(
                request.ShipmentId,
                userId,
                request.NewStatus,
                request.Description,
                request.City);

            var result = await _mediator.Send(command);
            return this.ToActionResult(result);
        }

        [HttpGet("track/{trackingNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> Track(string trackingNumber)
        {
            var query = new TrackShipmentQuery(trackingNumber);
            var result = await _mediator.Send(query);
            return this.ToActionResult(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetShipmentByIdQuery(id);
            var result = await _mediator.Send(query);
            return this.ToActionResult(result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Customer,Company")]
        public async Task<IActionResult> GetMyShipments()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetMyShipmentsQuery(userId);
            var result = await _mediator.Send(query);
            return this.ToActionResult(result);
        }
    }
}
