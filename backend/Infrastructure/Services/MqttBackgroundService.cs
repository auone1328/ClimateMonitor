using Application.Features.MeasurementFeatures.Create;
using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using System.Text;
using System.Text.Json;
using MQTTnet.Adapter;

namespace Infrastructure.Services
{
    public class MqttBackgroundService : BackgroundService
    {
        private readonly IMqttClientAccessor _clientAccessor;
        private readonly MqttOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttBackgroundService> _logger;

        public MqttBackgroundService(
            IMqttClientAccessor clientAccessor,
            IOptions<MqttOptions> options,
            IServiceScopeFactory scopeFactory,
            ILogger<MqttBackgroundService> logger)
        {
            _clientAccessor = clientAccessor;
            _options = options.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = _clientAccessor.Client;
            client.ApplicationMessageReceivedAsync += HandleMessageAsync;
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Host, _options.Port)
                .WithClientId($"climate-api-{Guid.NewGuid():N}")
                .WithProtocolVersion(_options.ProtocolVersion);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                builder.WithCredentials(_options.Username, _options.Password);
            }

            var clientOptions = builder.Build();

            var topic = $"{_options.TopicPrefix}+/telemetry";

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!client.IsConnected)
                    {
                        await client.ConnectAsync(clientOptions, stoppingToken);
                        await client.SubscribeAsync(topic, cancellationToken: stoppingToken);
                        _logger.LogInformation("MQTT subscribed to {Topic}", topic);
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (MqttConnectingFailedException ex)
                {
                    _logger.LogError(
                        ex,
                        "MQTT connection failed ({ResultCode} {ReasonString}), retrying in 5s",
                        ex.Result?.ResultCode,
                        ex.Result?.ReasonString);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MQTT connection failed, retrying in 5s");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs args)
        {
            var payload = args.ApplicationMessage.PayloadSegment.Count == 0
                ? string.Empty
                : Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);

            var mac = NormalizeMac(ExtractMacFromTopic(args.ApplicationMessage.Topic));
            if (string.IsNullOrWhiteSpace(mac))
                return;

            TelemetryPayload? data;
            try
            {
                data = JsonSerializer.Deserialize<TelemetryPayload>(payload, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse telemetry payload: {Payload}", payload);
                return;
            }

            if (data == null)
                return;

            using var scope = _scopeFactory.CreateScope();
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IDeviceRepository>();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var device = await deviceRepo.GetByMacAsync(mac);
            if (device == null)
            {
                _logger.LogWarning("Device not found for MAC {Mac} (topic {Topic})", mac, args.ApplicationMessage.Topic);
                return;
            }

            var relay = data.Relay ?? data.Heater ?? false;
            var heater = data.Heater ?? relay;
            var cooler = data.Cooler ?? false;
            var co2 = ConvertCo2ToPpm(data.CO2);

            await mediator.Send(new CreateMeasurementCommand(
                device.Id,
                data.Temperature,
                data.Humidity,
                co2,
                relay,
                heater,
                cooler,
                data.Timestamp));
        }

        private string ExtractMacFromTopic(string topic)
        {
            var prefix = _options.TopicPrefix;
            if (!topic.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var rest = topic.Substring(prefix.Length);
            var parts = rest.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return string.Empty;

            return parts[0];
        }

        private static string NormalizeMac(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
                return string.Empty;

            return mac
                .ToLowerInvariant()
                .Replace(":", "")
                .Replace("-", "")
                .Trim();
        }

        private static float ConvertCo2ToPpm(float raw)
        {
            // Two-point calibration from user:
            // raw=1549 -> 26 ppm, raw=1818 -> 400 ppm
            const float x1 = 1549f;
            const float y1 = 26f;
            const float x2 = 1818f;
            const float y2 = 400f;

            var k = (y2 - y1) / (x2 - x1);
            var b = y1 - k * x1;
            var ppm = k * raw + b;

            if (ppm < 0f) ppm = 0f;
            if (ppm > 5000f) ppm = 5000f;
            return ppm;
        }

        private class TelemetryPayload
        {
            public float Temperature { get; set; }
            public float Humidity { get; set; }
            public float CO2 { get; set; }
            public bool? Relay { get; set; }
            public bool? Heater { get; set; }
            public bool? Cooler { get; set; }
            public DateTime? Timestamp { get; set; }
        }
    }
}
