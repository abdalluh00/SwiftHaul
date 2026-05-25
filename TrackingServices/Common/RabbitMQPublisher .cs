using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
namespace TrackingServices.Common
{
   

    

    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMQPublisher(IConfiguration config)
        {
            var rabbitConfig = config.GetSection("RabbitMQ");

            var factory = new ConnectionFactory
            {
                HostName = rabbitConfig["Host"]!,
                UserName = rabbitConfig["Username"]!,
                Password = rabbitConfig["Password"]!
            };

            var retries = 10;
            while(retries > 0)
            {
                try
                {
                    _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                    _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                    Console.WriteLine("✅ RabbitMQ connected successfully.");
                    break;
                }
                catch (Exception ex)
                {
                    retries--;
                    Console.WriteLine($"⚠️ RabbitMQ not ready, retrying... ({retries} left). Error: {ex.Message}");
                    Thread.Sleep(3000);
                }
            };

            if (_connection is null || _channel is null)
                throw new Exception("❌ Could not connect to RabbitMQ after multiple retries.");
        }

        public async Task PublishAsync<T>(string queueName, T message)
        {
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body);
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
        }
    }
}
