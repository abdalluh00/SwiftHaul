using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrackingServices.Common;
using TrackingServices.Domain.Entities;
using TrackingServices.Infrastructure.Data;
using static TrackingServices.Application.DTOs.TrackingDTOs;

namespace TrackingServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrackingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RabbitMQPublisher _publisher;

        public TrackingController(AppDbContext context, RabbitMQPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }



        // Driver updates their location
        [HttpPost("location")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> UpdateLocation(UpdateLocationRequest request)
        {
            var driverId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var location = new DriverLocation
            {
                DriverId = driverId,
                ShipmentId = request.ShipmentId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CurrentCity = request.CurrentCity,
                StatusMessage = request.StatusMessage
            };

            _context.DriverLocations.Add(location);
            await _context.SaveChangesAsync();

            // Publish event to RabbitMQ
            await _publisher.PublishAsync("shipment.tracking", new ShipmentStatusEvent(
                request.ShipmentId,
                driverId,
                request.CurrentCity,
                request.StatusMessage,
                DateTime.UtcNow
            ));

            return Ok("Location updated and event published.");
        }



        // Get latest driver location for a shipment
        [HttpGet("shipment/{shipmentId}")]
        [Authorize(Roles = "Customer,Company,Admin")]
        public async Task<IActionResult> GetShipmentLocation(Guid shipmentId)
        {
            var location = await _context.DriverLocations
                .Where(d => d.ShipmentId == shipmentId)
                .OrderByDescending(d => d.RecordedAt)
                .FirstOrDefaultAsync();

            if (location is null)
                return NotFound("No location updates found for this shipment.");

            return Ok(new LocationResponse(
                location.DriverId,
                location.ShipmentId,
                location.Latitude,
                location.Longitude,
                location.CurrentCity,
                location.StatusMessage,
                location.RecordedAt
            ));
        }


        // Get full location history for a shipment
        [HttpGet("shipment/{shipmentId}/history")]
        [Authorize(Roles = "Customer,Company,Admin")]
        public async Task<IActionResult> GetLocationHistory(Guid shipmentId)
        {
            var history = await _context.DriverLocations
                .Where(d => d.ShipmentId == shipmentId)
                .OrderBy(d => d.RecordedAt)
                .Select(d => new LocationResponse(
                    d.DriverId,
                    d.ShipmentId,
                    d.Latitude,
                    d.Longitude,
                    d.CurrentCity,
                    d.StatusMessage,
                    d.RecordedAt))
                .ToListAsync();

            return Ok(history);
        }

    }
}
