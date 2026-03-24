using MQTTnet;
using MQTTnet.Client;

namespace Infrastructure.Services
{
    public interface IMqttClientAccessor
    {
        IMqttClient Client { get; }
    }

    public class MqttClientAccessor : IMqttClientAccessor
    {
        public IMqttClient Client { get; }

        public MqttClientAccessor()
        {
            var factory = new MqttFactory();
            Client = factory.CreateMqttClient();
        }
    }
}
