using Database;

namespace Configuration.Sqlite;

public class SqliteDbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    private static readonly string[] InitCommands =
    [
        "CREATE TABLE IF NOT EXISTS Settings (Name TEXT PRIMARY KEY, Value TEXT NOT NULL)",
        "INSERT OR IGNORE INTO Settings (Name, Value) VALUES ('Dependency:BaseUrl', 'http://localhost:5000')",
        "INSERT OR IGNORE INTO Settings (Name, Value) VALUES ('Dependency:ApiKey', '<apiKeyValue>')"
    ];

    public SqliteDbInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Initialize()
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = string.Join(";", InitCommands);

        connection.Open();
        command.ExecuteNonQuery();
    }
}