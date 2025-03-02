using Microsoft.Extensions.Configuration;

namespace Configuration.Services;

public sealed class AutoReloadingConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public AutoReloadingConfigurationProvider(ISettingsProvider settingsProvider, TimeSpan reloadInterval)
    {
        if (reloadInterval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(reloadInterval), "Reload interval must be greater than zero.");

        _settingsProvider = settingsProvider;
        _cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = _cancellationTokenSource.Token;
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(reloadInterval, cancellationToken);
                if (Reload())
                    OnReload();
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    public override void Load()
    {
        Reload();
    }

    private bool Reload()
    {
        var settings = _settingsProvider.GetSettings();
        if (AreDictionariesEqual(Data, settings))
            return false;

        Data = settings;
        return true;
    }

    private static bool AreDictionariesEqual(IDictionary<string, string?> left, Dictionary<string, string?> right)
    {
        if (left.Count != right.Count)
            return false;

        foreach (var (k, vLeft) in left)
        {
            if (!right.TryGetValue(k, out var vRight) ||
                !string.Equals(vLeft, vRight, StringComparison.Ordinal))
                return false;
        }

        return true;
    }
}