namespace Configuration.Services;

public interface ISettingsProvider
{
    Dictionary<string, string?> GetSettings();
}