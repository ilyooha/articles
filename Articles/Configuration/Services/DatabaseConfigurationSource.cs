using Database;
using Microsoft.Extensions.Configuration;

namespace Configuration.Services;

public class DatabaseConfigurationSource : IConfigurationSource, ISettingsProvider
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly TimeSpan _reloadInterval;

    public DatabaseConfigurationSource(IDbConnectionFactory dbConnectionFactory, TimeSpan reloadInterval)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _reloadInterval = reloadInterval;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new AutoReloadingConfigurationProvider(this, _reloadInterval);

    public Dictionary<string, string?> GetSettings()
    {
        Console.WriteLine("GetSettings is called");

        using var connection = _dbConnectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT Name, Value FROM Settings";

        connection.Open();

        using var reader = command.ExecuteReader();

        var settings = new Dictionary<string, string?>();
        while (reader.Read())
        {
            var name = reader.GetString(0);
            var value = reader.GetNullableString(1);

            settings[name] = value;
        }

        return settings;
    }
}