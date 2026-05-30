using NotificationServices.Domain.Entities;
using NotificationServices.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.Events;
using System.Text;
using System.Text.Json;

namespace NotificationServices.Consumers
{
    public class ShipmentCreatedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private IConnection? _connection;
        private IChannel? _channel;

        public ShipmentCreatedConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var rabbitConfig = _config.GetSection("RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"]!,
                UserName = rabbitConfig["Username"]!,
                Password = rabbitConfig["Password"]!
            };

            var retries = 10;
            while (retries > 0)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync(stoppingToken);
                    _channel = await _connection.CreateChannelAsync(
                        cancellationToken: stoppingToken);
                    Console.WriteLine("✅ ShipmentCreatedConsumer connected to RabbitMQ.");
                    break;
                }
                catch (Exception ex)
                {
                    retries--;
                    Console.WriteLine($"⚠️ Retrying... ({retries} left). {ex.Message}");
                    await Task.Delay(5000, stoppingToken);
                }
            }

            await _channel!.QueueDeclareAsync(
                queue: "shipment.created",
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, args) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(args.Body.ToArray());
                    var shipmentEvent = JsonSerializer
                        .Deserialize<ShipmentCreatedEvent>(body);

                    if (shipmentEvent is not null)
                        await HandleEventAsync(shipmentEvent);

                    await _channel.BasicAckAsync(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                    await _channel.BasicNackAsync(args.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "shipment.created",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }

        private async Task HandleEventAsync(ShipmentCreatedEvent shipmentEvent)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Notify all drivers — new shipment available
            var notification = new Notification
            {
                ShipmentId = shipmentEvent.ShipmentId,
                CustomerId = Guid.Empty, // all drivers
                Title = "New Shipment Available",
                Message = $"New shipment from {shipmentEvent.PickupCity} " +
                          $"to {shipmentEvent.DeliveryCity}. " +
                          $"Weight: {shipmentEvent.WeightKg}kg. " +
                          $"Tracking: {shipmentEvent.TrackingNumber}"
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Driver notification created: {notification.Message}");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null) await _channel.CloseAsync(cancellationToken);
            if (_connection is not null) await _connection.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
