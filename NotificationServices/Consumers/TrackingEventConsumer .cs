using NotificationServices.Application.DTOs;
using NotificationServices.Domain.Entities;
using NotificationServices.Infrastructure.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NotificationServices.Consumers
{
    public class TrackingEventConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private IConnection? _connection;
        private IChannel? _channel;

        public TrackingEventConsumer(
            IServiceScopeFactory scopeFactory,
            IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Connect to RabbitMQ
            var rabbitConfig = _config.GetSection("RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"]!,
                UserName = rabbitConfig["Username"]!,
                Password = rabbitConfig["Password"]!
            };

            IConnection? connection = null;
            var retries = 10;

            while(retries > 0)
            {
                try
                {
                    connection = await factory.CreateConnectionAsync(stoppingToken);
                    Console.WriteLine("✅ NotificationService connected to RabbitMQ.");
                    break;
                }
                catch (Exception ex)
                {
                    retries--;
                    Console.WriteLine($"⚠️ RabbitMQ not ready, retrying... ({retries} left). Error: {ex.Message}");
                    await Task.Delay(3000, stoppingToken);
                }
            };
            
            if (connection is null)
                throw new Exception("❌ Could not connect to RabbitMQ after multiple retries.");
            _connection = connection;
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // Declare the same queue TrackingService publishes to
            await _channel.QueueDeclareAsync(
                queue: "shipment.tracking",
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
                    var trackingEvent = JsonSerializer.Deserialize<ShipmentStatusEvent>(body);

                    if (trackingEvent is not null)
                        await HandleEventAsync(trackingEvent);

                    // Acknowledge message — tell RabbitMQ we processed it
                    await _channel.BasicAckAsync(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    // Reject and requeue on failure
                    await _channel.BasicNackAsync(args.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: "shipment.tracking",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            // Keep running until service stops
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }

        private async Task HandleEventAsync(ShipmentStatusEvent trackingEvent)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var notification = new Notification
            {
                ShipmentId = trackingEvent.ShipmentId,
                CustomerId = trackingEvent.DriverId, // will improve with Gateway
                Title = "Shipment Update",
                Message = $"Your shipment is now in {trackingEvent.CurrentCity}. " +
                          $"{trackingEvent.StatusMessage}"
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Notification saved: {notification.Message}");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
                await _channel.CloseAsync(cancellationToken);

            if (_connection is not null)
                await _connection.CloseAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}
