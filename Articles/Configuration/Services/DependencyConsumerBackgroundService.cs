using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Configuration.Services;

public class DependencyConsumerBackgroundService : BackgroundService
{
    private readonly IOptionsMonitor<DependencyOptions> _options;

    public DependencyConsumerBackgroundService(IOptionsMonitor<DependencyOptions> options)
    {
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            Console.WriteLine(
                $"[BackgroundService] BaseUrl: {_options.CurrentValue.BaseUrl}");
            Console.WriteLine(
                $"[BackgroundService] ApiKey: {_options.CurrentValue.ApiKey}");
        }
    }
}