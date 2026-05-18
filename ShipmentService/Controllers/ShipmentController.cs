using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShipmentService.Application.Dtos;
using ShipmentService.Common.Helpers;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.Entities.Events;
using ShipmentService.Infrastructure.Data;
using System.Security.Claims;

namespace ShipmentService.Controllers
{
   

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShipmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShipmentController(AppDbContext context)
        {
            _context = context;
        }

        // Customer or Company creates a shipment
        [HttpPost]
        [Authorize(Roles = "Customer,Company")]
        public async Task<IActionResult> CreateShipment(CreateShipmentRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = User.FindFirstValue(ClaimTypes.Role)!;

            var shipment = new Shipment
            {
                TrackingNumber = TrackingNumberHelper.Generate(),
                CustomerId = userId,
                CompanyId = userRole == "Company" ? userId : null,
                PickupCity = request.PickupCity,
                PickupAddress = request.PickupAddress,
                DeliveryCity = request.DeliveryCity,
                DeliveryAddress = request.DeliveryAddress,
                WeightKg = request.WeightKg,
                Description = request.Description,
                IsInstant = request.IsInstant,
                EstimatedDelivery = DateTime.UtcNow.AddDays(
                    GetEstimatedDays(request.PickupCity, request.DeliveryCity))
            };

            _context.Shipments.Add(shipment);

            // First event — shipment created
            _context.ShipmentEvents.Add(new Domain.Entities.Events.ShipmentEvent
            {
                ShipmentId = shipment.Id,
                EventType = "ShipmentCreated",
                Description = $"Shipment created from {request.PickupCity} to {request.DeliveryCity}",
                City = request.PickupCity,
                PerformedBy = userId
            });

            await _context.SaveChangesAsync();

            return Ok(new ShipmentResponse(
                shipment.Id,
                shipment.TrackingNumber,
                shipment.PickupCity,
                shipment.DeliveryCity,
                shipment.CurrentStatus.ToString(),
                shipment.CreatedAt,
                shipment.EstimatedDelivery
            ));
        }

        // Driver or Admin updates shipment status
        [HttpPut("status")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> UpdateStatus(UpdateStatusRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.Id == request.ShipmentId);

            if (shipment is null)
                return NotFound("Shipment not found.");

            var newStatus = (ShipmentStatus)request.NewStatus;

            // Validate status transition
            if (!IsValidTransition(shipment.CurrentStatus, newStatus))
                return BadRequest($"Cannot transition from {shipment.CurrentStatus} to {newStatus}.");

            // Update status
            shipment.CurrentStatus = newStatus;

            // Append event — never update, only append
            _context.ShipmentEvents.Add(new ShipmentEvent
            {
                ShipmentId = shipment.Id,
                EventType = newStatus.ToString(),
                Description = request.Description,
                City = request.City,
                PerformedBy = userId
            });

            await _context.SaveChangesAsync();

            return Ok($"Shipment status updated to {newStatus}.");
        }

        // Anyone can track by tracking number
        [HttpGet("track/{trackingNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> Track(string trackingNumber)
        {
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

            if (shipment is null)
                return NotFound("Tracking number not found.");

            var events = await _context.ShipmentEvents
                .Where(e => e.ShipmentId == shipment.Id)
                .OrderBy(e => e.OccurredAt)
                .Select(e => new ShipmentHistoryResponse(
                    e.EventType,
                    e.Description,
                    e.City,
                    e.OccurredAt))
                .ToListAsync();

            return Ok(new
            {
                TrackingNumber = shipment.TrackingNumber,
                CurrentStatus = shipment.CurrentStatus.ToString(),
                PickupCity = shipment.PickupCity,
                DeliveryCity = shipment.DeliveryCity,
                EstimatedDelivery = shipment.EstimatedDelivery,
                History = events
            });
        }

        // Customer sees their own shipments
        [HttpGet("my")]
        [Authorize(Roles = "Customer,Company")]
        public async Task<IActionResult> GetMyShipments()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var shipments = await _context.Shipments
                .Where(s => s.CustomerId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ShipmentResponse(
                    s.Id,
                    s.TrackingNumber,
                    s.PickupCity,
                    s.DeliveryCity,
                    s.CurrentStatus.ToString(),
                    s.CreatedAt,
                    s.EstimatedDelivery))
                .ToListAsync();

            return Ok(shipments);
        }

        // ── Private Helpers ──

        private static bool IsValidTransition(ShipmentStatus current, ShipmentStatus next)
        {
            return (current, next) switch
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

        private static int GetEstimatedDays(string pickup, string delivery)
        {
            // Same city
            if (pickup == delivery) return 1;

            // Far cities
            var farCities = new[] { "Tabuk", "Abha" };
            if (farCities.Contains(pickup) || farCities.Contains(delivery)) return 4;

            // Default
            return 2;
        }
    }
}
