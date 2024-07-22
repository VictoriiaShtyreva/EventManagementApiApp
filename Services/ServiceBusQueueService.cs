using Azure.Messaging.ServiceBus;

namespace EventManagementApi.Services
{
    public class ServiceBusQueueService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string? _queueName;

        public ServiceBusQueueService(ServiceBusClient serviceBusClient, IConfiguration configuration)
        {
            _serviceBusClient = serviceBusClient;
            _queueName = configuration["ServiceBus:QueueName"];
        }

        public async Task SendMessageAsync(ServiceBusMessage message)
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
            await sender.SendMessageAsync(message);
        }
    }
}