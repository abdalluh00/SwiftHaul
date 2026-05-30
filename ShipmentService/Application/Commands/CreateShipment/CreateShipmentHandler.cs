using SharedKernel.Common;
using SharedKernel.CQRS;
using SharedKernel.Events;
using ShipmentService.Application.Dtos;
using ShipmentService.Common;
using ShipmentService.Common.Helpers;
using ShipmentService.Domain.Entities;
using ShipmentService.Domain.Entities.Events;
using ShipmentService.Infrastructure.Data;

namespace ShipmentService.Application.Commands.CreateShipment
{
    public class CreateShipmentHandler
     : ICommandHandler<CreateShipmentCommand, ShipmentResponse>
    {
        private readonly AppDbContext _context;
        private readonly RabbitMQPublisher _publisher;  
        public CreateShipmentHandler(AppDbContext context, RabbitMQPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public async Task<Result<ShipmentResponse>> Handle(
            CreateShipmentCommand request,
            CancellationToken cancellationToken)
        {
            var shipment = new Shipment
            {
                TrackingNumber = TrackingNumberHelper.Generate(),
                CustomerId = request.CustomerId,
                CompanyId = request.CustomerRole == "Company" ? request.CustomerId : null,
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

            _context.ShipmentEvents.Add(new ShipmentEvent
            {
                ShipmentId = shipment.Id,
                EventType = "ShipmentCreated",
                Description = $"Shipment created from {request.PickupCity} to {request.DeliveryCity}",
                City = request.PickupCity,
                PerformedBy = request.CustomerId
            });

            await _context.SaveChangesAsync(cancellationToken);
            // Publish event to RabbitMQ after saving
            await _publisher.PublishAsync("shipment.created", new ShipmentCreatedEvent(
                shipment.Id,
                shipment.TrackingNumber,
                shipment.PickupCity,
                shipment.DeliveryCity,
                shipment.WeightKg,
                request.CustomerId,
                shipment.CreatedAt
            ));

            Console.WriteLine($"✅ ShipmentCreated event published: {shipment.TrackingNumber}");

            return Result.Success(new ShipmentResponse(
                shipment.Id,
                shipment.TrackingNumber,
                shipment.PickupCity,
                shipment.DeliveryCity,
                shipment.CurrentStatus.ToString(),
                shipment.CreatedAt,
                shipment.EstimatedDelivery));
        }

        private static int GetEstimatedDays(string pickup, string delivery)
        {
            if (pickup == delivery) return 1;
            var farCities = new[] { "Tabuk", "Abha" };
            if (farCities.Contains(pickup) || farCities.Contains(delivery)) return 4;
            return 2;
        }
    }
}
