using MQTTnet.Formatter;

namespace Infrastructure
{
    public class MqttOptions
    {
        public string Host { get; set; } = "broker.hivemq.com";
        public int Port { get; set; } = 1883;
        public string TopicPrefix { get; set; } = "cs2026_climate_7A3B9F2C/";
        public MqttProtocolVersion ProtocolVersion { get; set; } = MqttProtocolVersion.V311;
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
