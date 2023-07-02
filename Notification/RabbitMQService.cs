using Microsoft.AspNetCore.Connections;
using Notification.DTO;
using RabbitMQ.Client;

namespace Notification
{
    public class RabbitMQService
    {
        private readonly RabbitMQConfigDTO _rabbitMQConfig;
        private readonly IConnection _connection;

        public RabbitMQService(RabbitMQConfigDTO rabbitMQConfig)
        {
            _rabbitMQConfig = rabbitMQConfig;
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQConfig.HostName,
                UserName = _rabbitMQConfig.UserName,
                Password = _rabbitMQConfig.Password
            };
            _connection = factory.CreateConnection();
        }
    }
}
