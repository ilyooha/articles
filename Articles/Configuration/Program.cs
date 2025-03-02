// See https://aka.ms/new-console-template for more information

using Configuration;
using Configuration.Services;
using Configuration.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

var builder = Host.CreateApplicationBuilder();

var connectionFactory = new SqliteDbConnectionFactory(
    builder.Configuration.GetConnectionString("Settings")
    ?? throw new InvalidOperationException("Settings connection string not found."));

new SqliteDbInitializer(connectionFactory).Initialize();

builder.Configuration.Sources.Add(
    new DatabaseConfigurationSource(connectionFactory, TimeSpan.FromSeconds(15)));

var section = builder.Configuration.GetSection("Dependency");

builder.Services.Configure<DependencyOptions>(section);
builder.Services.AddHostedService<DependencyConsumerBackgroundService>();

using var sectionSubscription = ChangeToken.OnChange(
    section.GetReloadToken,
    () =>
    {
        var options = section.Get<DependencyOptions>();
        Console.WriteLine($"[ChangeToken.OnChange] Section is updated. BaseUrl: {options?.BaseUrl}");
    });

var otherSection = builder.Configuration.GetSection("ConnectionStrings");
using var otherSectionSubscription = ChangeToken.OnChange(
    otherSection.GetReloadToken,
    () => { Console.WriteLine("[ChangeToken.OnChange] Other section is updated"); });


Task.Run(async () =>
{
    while (true)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        var baseUrl = builder.Configuration.GetSection("Dependency:BaseUrl").Get<string>();
        Console.WriteLine($"[Section] BaseUrl: {baseUrl}");
    }
});

var host = builder.Build();

host.Run();