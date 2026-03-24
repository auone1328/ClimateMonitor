using Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CleanupOptions _options;
        private readonly ILogger<CleanupBackgroundService> _logger;

        public CleanupBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<CleanupOptions> options,
            ILogger<CleanupBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.IntervalMinutes));
            using var timer = new PeriodicTimer(interval);

            _logger.LogInformation("Cleanup service started. Interval: {Interval} min, Retention: {Retention} days", interval.TotalMinutes, _options.RetentionDays);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    var cutoff = DateTime.UtcNow.AddDays(-_options.RetentionDays);
                    using var scope = _scopeFactory.CreateScope();
                    var measurementRepo = scope.ServiceProvider.GetRequiredService<IMeasurementRepository>();
                    var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                    var deletedMeasurements = await measurementRepo.DeleteOlderThanAsync(cutoff);
                    var deletedNotifications = await notificationRepo.DeleteOlderThanAsync(cutoff);

                    _logger.LogInformation(
                        "Cleanup completed. Measurements deleted: {Measurements}. Notifications deleted: {Notifications}.",
                        deletedMeasurements,
                        deletedNotifications);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cleanup failed");
                }
            }
        }
    }
}
