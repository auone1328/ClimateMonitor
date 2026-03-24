namespace Application.Interfaces.Services
{
    public interface IMqttPublisher
    {
        Task PublishRelayCommandAsync(string macAddress, bool relayState);
        Task PublishHeaterCommandAsync(string macAddress, bool heaterState);
        Task PublishCoolerCommandAsync(string macAddress, bool coolerState);
    }
}
