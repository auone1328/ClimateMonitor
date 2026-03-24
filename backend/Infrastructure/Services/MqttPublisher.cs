using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace Infrastructure.Services
{
    public class MqttPublisher : IMqttPublisher
    {
        private readonly IMqttClientAccessor _clientAccessor;
        private readonly MqttOptions _options;
        private readonly ILogger<MqttPublisher> _logger;

        public MqttPublisher(
            IMqttClientAccessor clientAccessor,
            IOptions<MqttOptions> options,
            ILogger<MqttPublisher> logger)
        {
            _clientAccessor = clientAccessor;
            _options = options.Value;
            _logger = logger;
        }

        public async Task PublishRelayCommandAsync(string macAddress, bool relayState)
        {
            await PublishHeaterCommandAsync(macAddress, relayState);
        }

        public async Task PublishHeaterCommandAsync(string macAddress, bool heaterState)
        {
            await PublishAsync(macAddress, "heater/set", $"{{\"heater\":{(heaterState ? "true" : "false")}}}");
        }

        public async Task PublishCoolerCommandAsync(string macAddress, bool coolerState)
        {
            await PublishAsync(macAddress, "cooler/set", $"{{\"cooler\":{(coolerState ? "true" : "false")}}}");
        }

        private async Task PublishAsync(string macAddress, string topicSuffix, string payload)
        {
            var client = _clientAccessor.Client;
            if (!client.IsConnected)
            {
                _logger.LogWarning("MQTT client is not connected, command skipped");
                return;
            }

            var topic = $"{_options.TopicPrefix}{NormalizeMac(macAddress)}/{topicSuffix}";
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await client.PublishAsync(message);
        }

        private static string NormalizeMac(string mac)
        {
            return mac.ToLowerInvariant().Replace(":", "").Replace("-", "");
        }
    }
}
